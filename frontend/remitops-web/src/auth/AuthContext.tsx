import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import api from './api'
import {
  clearToken,
  getStoredUser,
  getToken,
  parseToken,
  setStoredUser,
  setToken,
  type AuthUser,
} from './auth'

type LoginPayload = {
  email: string
  password: string
}

type AuthContextType = {
  user: AuthUser | null
  token: string | null
  loading: boolean
  login: (payload: LoginPayload) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | null>(null)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setTokenState] = useState<string | null>(getToken())
  const [user, setUser] = useState<AuthUser | null>(getStoredUser())
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const currentToken = getToken()
    if (currentToken && !user) {
      setUser(parseToken(currentToken))
    }
    setLoading(false)
  }, [user])

  const login = async ({ email, password }: LoginPayload) => {
    const res = await api.post('/api/auth/login', { email, password })
    const jwt = res.data.token as string
    const parsed = parseToken(jwt)

    setToken(jwt)
    setStoredUser(parsed)
    setTokenState(jwt)
    setUser(parsed)
  }

  const logout = () => {
    clearToken()
    setTokenState(null)
    setUser(null)
  }

  const value = useMemo(
    () => ({ user, token, loading, login, logout }),
    [user, token, loading]
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}

