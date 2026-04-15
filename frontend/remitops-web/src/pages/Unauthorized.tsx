import { useNavigate } from "react-router-dom";
import AppButton from "../components/ui/AppButton";

export default function Unauthorized() {
  const navigate = useNavigate();

  return (
    <div className="auth-centered">
      <div className="card unauthorized-card">
        <div className="page-eyebrow">Access control</div>
        <h1 className="unauthorized-title">Unauthorized</h1>
        <p className="unauthorized-copy">
          You do not have permission to view this page. Please return to a page
          available for your role or sign in with an account that has access.
        </p>

        <div className="hero-actions">
          <AppButton variant="primary" onClick={() => navigate("/")}>
            Go Home
          </AppButton>

          <AppButton variant="secondary" onClick={() => navigate("/login")}>
            Back to Login
          </AppButton>
        </div>
      </div>
    </div>
  );
}