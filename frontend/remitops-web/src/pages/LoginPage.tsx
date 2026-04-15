import { FormEvent, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext'
import {
    getStoredUser,
    isEndUser,
    isOrgUnitAdmin,
    isPlatformAdmin,
} from '../auth/auth'

export default function LoginPage() {
    const { login } = useAuth()
    const navigate = useNavigate()
    const location = useLocation()

    const [email, setEmail] = useState('platform.admin@remitops.local')
    const [password, setPassword] = useState('RemitOps@12345')
    const [error, setError] = useState('')
    const [submitting, setSubmitting] = useState(false)

    async function onSubmit(e: FormEvent) {
        e.preventDefault()
        setError('')
        setSubmitting(true)

        try {
            await login({ email, password })

            const currentUser = getStoredUser()
            const from = (location.state as any)?.from?.pathname

            if (from) {
                navigate(from, { replace: true })
                return
            }

            if (isPlatformAdmin(currentUser)) navigate('/admin', { replace: true })
            else if (isOrgUnitAdmin(currentUser)) navigate('/org', { replace: true })
            else if (isEndUser(currentUser)) navigate('/me', { replace: true })
            else navigate('/unauthorized', { replace: true })
        } catch (err: any) {
            setError(err?.response?.data?.message || 'Login failed')
        } finally {
            setSubmitting(false)
        }
    }

    return (
        <div className="login-shell">
            <div className="login-layout">
                <section className="login-brand-panel">
                    <div className="login-brand-overlay">
                        <img
                            src="/dahabshiil-logo.png"
                            alt="Dahabshiil-inspired reference"
                            className="brand-logo"
                        />
                        <div className="login-badge">RemitOps</div>
                        <h1>Dahabshiil-inspired remittance platform</h1>
                        <p>
                            Multi-tenant access, KYC workflow, source and destination queues,
                            and role-based dashboards for remittance operations.
                        </p>

                        <div className="login-brand-points">
                            <div>Platform administration</div>
                            <div>Branch and org-unit control</div>
                            <div>KYC and transaction workflows</div>
                        </div>
                    </div>
                </section>

                <section className="login-form-panel">
                    <form className="auth-card" onSubmit={onSubmit}>
                        <div className="auth-heading">
                            <div className="auth-brand">RemitOps</div>
                            <h2>Sign in</h2>
                            <p>Secure access for operations, compliance, and reporting.</p>
                        </div>

                        <input
                            type="email"
                            placeholder="Email"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />

                        <input
                            type="password"
                            placeholder="Password"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                        />

                        {error ? <div className="auth-error">{error}</div> : null}

                        <button type="submit" className="btn-primary" disabled={submitting}>
                            {submitting ? 'Signing in...' : 'Sign in'}
                        </button>
                    </form>
                </section>
            </div>
        </div>
    )
}