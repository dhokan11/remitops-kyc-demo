import { createBrowserRouter, Navigate } from "react-router-dom";
import AdminLayout from "../components/layout/AdminLayout";
import AdminHome from "../pages/AdminHome";
import TenantsPage from "../features/admin/pages/TenantsPage";
import OrgUnitsPage from "../features/admin/pages/OrgUnitsPage";
import UsersPage from "../features/admin/pages/UsersPage";
import KycReviewPage from "../features/admin/pages/KycReviewPage";
import TransactionsPage from "../features/admin/pages/TransactionsPage";
import AuditTrailPage from "../features/admin/pages/AuditTrailPage";
import ReportsPage from "../features/admin/pages/ReportsPage";
import SettingsPage from "../features/admin/pages/SettingsPage";
import LoginPage from "../pages/LoginPage";
import Unauthorized from "../pages/Unauthorized";

function RequireRole({
  children,
  roles,
}: {
  children: JSX.Element;
  roles: string[];
}) {
  const user = JSON.parse(localStorage.getItem("auth_user") || "null");
  if (!user?.token) return <Navigate to="/login" replace />;
  if (!roles.includes(user.role)) return <Navigate to="/unauthorized" replace />;
  return children;
}

export const router = createBrowserRouter([
  { path: "/", element: <Navigate to="/login" replace /> },
  { path: "/login", element: <LoginPage /> },
  { path: "/unauthorized", element: <Unauthorized /> },
  {
    path: "/admin",
    element: (
      <RequireRole roles={["PlatformAdmin"]}>
        <AdminLayout />
      </RequireRole>
    ),
    children: [
      { index: true, element: <AdminHome /> },
      { path: "tenants", element: <TenantsPage /> },
      { path: "org-units", element: <OrgUnitsPage /> },
      { path: "users", element: <UsersPage /> },
      { path: "kyc-review", element: <KycReviewPage /> },
      { path: "transactions", element: <TransactionsPage /> },
      { path: "audit-trail", element: <AuditTrailPage /> },
      { path: "reports", element: <ReportsPage /> },
      { path: "settings", element: <SettingsPage /> },
    ],
  },
]);