import { NavLink } from "react-router-dom";
import AppButton from "../ui/AppButton";

type AppSidebarProps = {
  mobileOpen?: boolean;
  onClose?: () => void;
  onLogout?: () => void;
};

const items = [
  { to: "/admin", label: "Overview", end: true },
  { to: "/admin/tenants", label: "Tenants" },
  { to: "/admin/org-units", label: "Org Units" },
  { to: "/admin/users", label: "Users" },
  { to: "/admin/kyc-review", label: "KYC Review" },
  { to: "/admin/transactions", label: "Transactions" },
  { to: "/admin/reports", label: "Reports" },
  { to: "/admin/settings", label: "Settings" },
  { to: "/admin/audit-trail", label: "Audit Trail" },
];

export default function AppSidebar({
  mobileOpen = false,
  onClose,
  onLogout,
}: AppSidebarProps) {
  return (
    <aside
      id="admin-sidebar"
      className={`sidebar ${mobileOpen ? "mobile-open" : ""}`}
    >
      <div>
        <div className="sidebar-top">
          <img
            src="/dahabshiil-logo.png"
            alt="RemitOps"
            className="sidebar-logo"
          />
          <div className="sidebar-brand">RemitOps</div>
          <div className="sidebar-subtitle">Platform Admin</div>
        </div>

        <nav className="sidebar-nav" aria-label="Platform admin navigation">
          {items.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `sidebar-item ${isActive ? "active" : ""}`.trim()
              }
              onClick={onClose}
            >
              {item.label}
            </NavLink>
          ))}
        </nav>
      </div>

      <div className="sidebar-bottom">
        <AppButton
          variant="secondary"
          className="sidebar-logout"
          onClick={onLogout}
        >
          Logout
        </AppButton>
      </div>
    </aside>
  );
}