import { jwtDecode } from 'jwt-decode'

export type AuthUser = {
  sub?: string
  email?: string
  role?: string | string[]
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string | string[]
  tenant_id?: string
  org_unit_id?: string
  user_type?: string
  registration_status?: string
}

export function getToken() {
  return localStorage.getItem('remitops_token')
}

export function setToken(token: string) {
  localStorage.setItem('remitops_token', token)
}

export function clearToken() {
  localStorage.removeItem('remitops_token')
  localStorage.removeItem('remitops_user')
}

export function getStoredUser() {
  const raw = localStorage.getItem('remitops_user')
  return raw ? JSON.parse(raw) : null
}

export function setStoredUser(user: unknown) {
  localStorage.setItem('remitops_user', JSON.stringify(user))
}

export function parseToken(token: string): AuthUser | null {
  try {
    return jwtDecode<AuthUser>(token)
  } catch {
    return null
  }
}

export function getRoles(user: AuthUser | null): string[] {
  if (!user) return []

  const claimRole =
    user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']

  if (Array.isArray(claimRole)) return claimRole
  if (typeof claimRole === 'string') return [claimRole]

  if (Array.isArray(user.role)) return user.role
  if (typeof user.role === 'string') return [user.role]

  return []
}

export function isPlatformAdmin(user: AuthUser | null) {
  return getRoles(user).includes('PlatformAdmin')
}

export function isOrgUnitAdmin(user: AuthUser | null) {
  return getRoles(user).includes('OrgUnitAdmin')
}

export function isEndUser(user: AuthUser | null) {
  return getRoles(user).includes('EndUser')
}