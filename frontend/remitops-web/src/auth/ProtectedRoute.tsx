import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useAuth } from './AuthContext'

export default function ProtectedRoute({
  roles,
}: {
  roles?: string[]
}) {
  const { user, token, loading } = useAuth()
  const location = useLocation()

  if (loading) return <div className="state-msg">Checking session...</div>

  if (!token || !user) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  const rawRole =
    user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? user.role

  const userRoles = Array.isArray(rawRole)
    ? rawRole
    : rawRole
    ? [rawRole]
    : []

  if (roles && !roles.some((r) => userRoles.includes(r))) {
    return <Navigate to="/unauthorized" replace />
  }

  return <Outlet />
}