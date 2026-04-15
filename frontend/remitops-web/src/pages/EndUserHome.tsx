import { useAuth } from '../auth/AuthContext'

export default function EndUserHome() {
  const { logout } = useAuth()

  return (
    <div className="page">
      <div className="panel">
        <h2>End User</h2>
        <p>Track transactions, upload KYC, and view personal reports.</p>
        <button onClick={logout}>Logout</button>
      </div>
    </div>
  )
}