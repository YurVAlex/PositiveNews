/**
 * Global authentication state: JWT tokens, user profile, and feed-preference sync on login.
 * Wraps the app in main.tsx so any component can call useAuth().
 */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react'
import { getCurrentUser, login as loginApi, refreshToken as refreshTokenApi, register as registerApi } from '../api/auth-api'
import type { AuthResponse } from '../api/types'
import { getFeedPreferences, putFeedPreferences } from '../api/preferences-api'
import type { UserProfileResponse } from '../api/types'
import {
  clearFeedPrefsDraft,
  clearLocalFeedPreferences,
  loadFeedPrefsDraft,
  preferencesFromApiResponse,
  saveLastSavedPreferenceParams,
  serializePreferenceSnapshot,
  snapshotToApiRequest,
  type FeedPreferencesSnapshot,
} from '../utils/feed-preferences-url'

const AUTH_TOKEN_KEY = 'positiveNews.accessToken'
const REFRESH_TOKEN_KEY = 'positiveNews.refreshToken'
const TOKEN_EXPIRY_KEY = 'positiveNews.tokenExpiry'

type AuthContextValue = {
  isLoading: boolean
  token: string | null
  refreshToken: string | null
  user: UserProfileResponse | null
  isAuthenticated: boolean
  pendingServerPreferences: FeedPreferencesSnapshot | null
  clearPendingServerPreferences: () => void
  login: (email: string, password: string) => Promise<void>
  register: (email: string, name: string, password: string) => Promise<void>
  logout: () => void
  refreshTokens: () => Promise<AuthResponse>
}

const AuthContext = createContext<AuthContextValue | null>(null)

/** Provides auth state and actions to the component tree. */
export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(null)
  const [refreshToken, setRefreshToken] = useState<string | null>(null)
  const [user, setUser] = useState<UserProfileResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [pendingServerPreferences, setPendingServerPreferences] = useState<FeedPreferencesSnapshot | null>(null)

  /** Clears prefs queued for FeedPage after they have been applied to the URL. */
  const clearPendingServerPreferences = useCallback(() => {
    setPendingServerPreferences(null)
  }, [])

  /** Fetches saved feed filters from the API and exposes them to FeedPage via pendingServerPreferences. */
  const loadServerPreferences = useCallback(async (accessToken: string, userId: number) => {
    const prefs = await getFeedPreferences(accessToken)
    const snapshot = preferencesFromApiResponse(prefs)
    setPendingServerPreferences(snapshot)
    saveLastSavedPreferenceParams(userId, serializePreferenceSnapshot(snapshot))
  }, [])

  /** Exchanges the stored refresh token for a new access token; logs out on failure. */
  const refreshTokens = useCallback(async () => {
    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
    if (!storedRefreshToken) {
      throw new Error('No refresh token available')
    }

    try {
      const response = await refreshTokenApi(storedRefreshToken)
      localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
      localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken)
      localStorage.setItem(TOKEN_EXPIRY_KEY, response.expiresAtUtc)
      setToken(response.accessToken)
      setRefreshToken(response.refreshToken)
      setUser(response.user)
      return response
    } catch (error) {
      // If refresh fails, clear tokens and log out
      localStorage.removeItem(AUTH_TOKEN_KEY)
      localStorage.removeItem(REFRESH_TOKEN_KEY)
      localStorage.removeItem(TOKEN_EXPIRY_KEY)
      setToken(null)
      setRefreshToken(null)
      setUser(null)
      throw error
    }
  }, [])

  // Proactively refresh before expiry so API calls do not start failing mid-session.
  useEffect(() => {
    if (!token) return

    const checkTokenExpiry = () => {
      const expiry = localStorage.getItem(TOKEN_EXPIRY_KEY)
      if (!expiry) return

      const expiryTime = new Date(expiry).getTime()
      const now = Date.now()
      const timeUntilExpiry = expiryTime - now

      // Refresh token 5 minutes before it expires
      if (timeUntilExpiry < 5 * 60 * 1000 && timeUntilExpiry > 0) {
        refreshTokens().catch(() => {
          // Refresh failed, user will need to log in again
        })
      }
    }

    const interval = setInterval(checkTokenExpiry, 60 * 1000) // Check every minute
    return () => clearInterval(interval)
  }, [token, refreshTokens])

  // On first load, restore session from localStorage and validate with /api/auth/me.
  useEffect(() => {
    const storedToken = localStorage.getItem(AUTH_TOKEN_KEY)
    const storedRefreshToken = localStorage.getItem(REFRESH_TOKEN_KEY)
    if (!storedToken) {
      setIsLoading(false)
      return
    }

    setToken(storedToken)
    setRefreshToken(storedRefreshToken)
    getCurrentUser(storedToken)
      .then(async (profile) => {
        setUser(profile)
        await loadServerPreferences(storedToken, profile.id)
      })
      .catch(async () => {
        // If current user fails, try to refresh the token
        if (storedRefreshToken) {
          try {
            await refreshTokens()
            const newToken = localStorage.getItem(AUTH_TOKEN_KEY)
            if (newToken) {
              const profile = await getCurrentUser(newToken)
              setUser(profile)
              await loadServerPreferences(newToken, profile.id)
            }
          } catch {
            // Refresh failed, clear tokens
            localStorage.removeItem(AUTH_TOKEN_KEY)
            localStorage.removeItem(REFRESH_TOKEN_KEY)
            localStorage.removeItem(TOKEN_EXPIRY_KEY)
            setToken(null)
            setRefreshToken(null)
            setUser(null)
          }
        } else {
          localStorage.removeItem(AUTH_TOKEN_KEY)
          setToken(null)
          setUser(null)
        }
      })
      .finally(() => setIsLoading(false))
  }, [loadServerPreferences, refreshTokens])

  /** Signs in, persists tokens, and loads server-side feed preferences. */
  const login = useCallback(
    async (email: string, password: string) => {
      const response = await loginApi(email, password)
      localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
      localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken)
      localStorage.setItem(TOKEN_EXPIRY_KEY, response.expiresAtUtc)
      setToken(response.accessToken)
      setRefreshToken(response.refreshToken)
      setUser(response.user)
      await loadServerPreferences(response.accessToken, response.user.id)
    },
    [loadServerPreferences],
  )

  /**
   * Creates an account and signs in. If the user configured feed filters while
   * anonymous (session draft), those are uploaded instead of loading from the server.
   */
  const register = useCallback(
    async (email: string, name: string, password: string) => {
      const response = await registerApi(email, name, password)
      localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
      localStorage.setItem(REFRESH_TOKEN_KEY, response.refreshToken)
      localStorage.setItem(TOKEN_EXPIRY_KEY, response.expiresAtUtc)
      setToken(response.accessToken)
      setRefreshToken(response.refreshToken)
      setUser(response.user)

      const draft = loadFeedPrefsDraft()
      if (draft) {
        await putFeedPreferences(response.accessToken, snapshotToApiRequest(draft))
        setPendingServerPreferences(draft)
        saveLastSavedPreferenceParams(response.user.id, serializePreferenceSnapshot(draft))
        clearFeedPrefsDraft()
      } else {
        await loadServerPreferences(response.accessToken, response.user.id)
      }
    },
    [loadServerPreferences],
  )

  /** Clears tokens, user state, and all session-scoped feed preference storage. */
  const logout = useCallback(() => {
    localStorage.removeItem(AUTH_TOKEN_KEY)
    localStorage.removeItem(REFRESH_TOKEN_KEY)
    localStorage.removeItem(TOKEN_EXPIRY_KEY)
    setToken(null)
    setRefreshToken(null)
    setUser(null)
    setPendingServerPreferences(null)
    clearLocalFeedPreferences()
  }, [])

  // Stable context object reference avoids re-rendering all useAuth() consumers on unrelated provider renders.
  const value = useMemo<AuthContextValue>(
    () => ({
      isLoading,
      token,
      refreshToken,
      user,
      isAuthenticated: Boolean(token && user),
      pendingServerPreferences,
      clearPendingServerPreferences,
      login,
      register,
      logout,
      refreshTokens,
    }),
    [
      isLoading,
      token,
      refreshToken,
      user,
      pendingServerPreferences,
      clearPendingServerPreferences,
      login,
      register,
      logout,
      refreshTokens,
    ],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

/** Hook to read auth state and actions; must be used inside AuthProvider. */
export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
