import { useAuth } from '../auth/AuthContext'

export default function OrgUnitHome() {
  const { logout } = useAuth()

  return (
    <div className="page">
      <div className="panel">
        <h2>Org Unit Admin</h2>
        <p>Access source queue, destination queue, compliance review, and reports.</p>
        <button onClick={logout}>Logout</button>
      </div>
    </div>
  )
}