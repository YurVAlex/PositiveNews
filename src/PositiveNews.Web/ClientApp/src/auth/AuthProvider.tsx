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
import type { UserProfileResponse } from '../api/types'

const AUTH_TOKEN_KEY = 'positiveNews.accessToken'

type AuthContextValue = {
  isLoading: boolean
  token: string | null
  user: UserProfileResponse | null
  isAuthenticated: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, name: string, password: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: PropsWithChildren) {
  const [token, setToken] = useState<string | null>(null)
  const [user, setUser] = useState<UserProfileResponse | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  useEffect(() => {
    const storedToken = localStorage.getItem(AUTH_TOKEN_KEY)
    if (!storedToken) {
      setIsLoading(false)
      return
    }

    setToken(storedToken)
    getCurrentUser(storedToken)
      .then((profile) => {
        setUser(profile)
      })
      .catch(() => {
        localStorage.removeItem(AUTH_TOKEN_KEY)
        setToken(null)
        setUser(null)
      })
      .finally(() => setIsLoading(false))
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const response = await loginApi(email, password)
    localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
    setToken(response.accessToken)
    setUser(response.user)
  }, [])

  const register = useCallback(async (email: string, name: string, password: string) => {
    const response = await registerApi(email, name, password)
    localStorage.setItem(AUTH_TOKEN_KEY, response.accessToken)
    setToken(response.accessToken)
    setUser(response.user)
  }, [])

  const logout = useCallback(() => {
    localStorage.removeItem(AUTH_TOKEN_KEY)
    setToken(null)
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      isLoading,
      token,
      user,
      isAuthenticated: Boolean(token && user),
      login,
      register,
      logout,
    }),
    [isLoading, token, user, login, register, logout],
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
