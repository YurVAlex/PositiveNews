import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react'
import { getCurrentUser, login as loginApi, register as registerApi } from '../api/auth-api'
import { getFeedPreferences, putFeedPreferences } from '../api/preferences-api'
import type { UserProfileResponse } from '../api/types'
import {
  clearFeedPrefsDraft,
  loadFeedPrefsDraft,
  preferencesFromApiResponse,
  snapshotToApiRequest,
  type FeedPreferencesSnapshot,
} from '../utils/feed-preferences-url'

const AUTH_TOKEN_KEY = 'positiveNews.accessToken'

type AuthContextValue = {
  isLoading: boolean
  token: string | null
  user: UserProfileResponse | null
  isAuthenticated: boolean
  pendingServerPreferences: FeedPreferencesSnapshot | null
  clearPendingServerPreferences: () => void
  login: (email: string, password: string) => Promise<void>
  register: (email: string, name: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(null)
  const [user, setUser] = useState<UserProfileResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [pendingServerPreferences, setPendingServerPreferences] = useState<FeedPreferencesSnapshot | null>(null)

  const clearPendingServerPreferences = useCallback(() => {
    setPendingServerPreferences(null)
  }, [])

  const loadServerPreferences = useCallback(async (accessToken: string) => {
    const prefs = await getFeedPreferences(accessToken)
    setPendingServerPreferences(preferencesFromApiResponse(prefs))
  }, [])

  useEffect(() => {
    const storedToken = localStorage.getItem(AUTH_TOKEN_KEY)
    if (!storedToken) {
      setIsLoading(false)
      return
    }

    setToken(storedToken)
    getCurrentUser(storedToken)
      .then(async (profile) => {
        setUser(profile)
        await loadServerPreferences(storedToken)
      })
      .catch(() => {
        localStorage.removeItem(AUTH_TOKEN_KEY)
        setToken(null)
        setUser(null)
      })
      .finally(() => setIsLoading(false))
  }, [loadServerPreferences])

  const login = useCallback(
    async (email: string, password: string) => {
      const response = await loginApi(email, password)
      localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
      setToken(response.accessToken)
      setUser(response.user)
      await loadServerPreferences(response.accessToken)
    },
    [loadServerPreferences],
  )

  const register = useCallback(
    async (email: string, name: string, password: string) => {
      const response = await registerApi(email, name, password)
      localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
      setToken(response.accessToken)
      setUser(response.user)

      const draft = loadFeedPrefsDraft()
      if (draft) {
        await putFeedPreferences(response.accessToken, snapshotToApiRequest(draft))
        setPendingServerPreferences(draft)
        clearFeedPrefsDraft()
      } else {
        await loadServerPreferences(response.accessToken)
      }
    },
    [loadServerPreferences],
  )

  const logout = useCallback(() => {
    localStorage.removeItem(AUTH_TOKEN_KEY)
    setToken(null)
    setUser(null)
    setPendingServerPreferences(null)
    clearFeedPrefsDraft()
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      isLoading,
      token,
      user,
      isAuthenticated: Boolean(token && user),
      pendingServerPreferences,
      clearPendingServerPreferences,
      login,
      register,
      logout,
    }),
    [
      isLoading,
      token,
      user,
      pendingServerPreferences,
      clearPendingServerPreferences,
      login,
      register,
      logout,
    ],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
