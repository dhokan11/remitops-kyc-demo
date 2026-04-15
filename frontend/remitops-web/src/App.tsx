import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider } from "./auth/AuthContext";
import ProtectedRoute from "./auth/ProtectedRoute";
import LoginPage from "./pages/LoginPage";
import AdminHome from "./pages/AdminHome";
import OrgUnitHome from "./pages/OrgUnitHome";
import EndUserHome from "./pages/EndUserHome";
import Unauthorized from "./pages/Unauthorized";

import AdminLayout from "./components/layout/AdminLayout";

import TenantsPage from "./features/admin/pages/TenantsPage";
import OrgUnitsPage from "./features/admin/pages/OrgUnitsPage";
import UsersPage from "./features/admin/pages/UsersPage";
import KycReviewPage from "./features/admin/pages/KycReviewPage";
import TransactionsPage from "./features/admin/pages/TransactionsPage";
import AuditTrailPage from "./features/admin/pages/AuditTrailPage";
import ReportsPage from "./features/admin/pages/ReportsPage";
import SettingsPage from "./features/admin/pages/SettingsPage";

import "./App.css";
import "./styles/admin.css";

export default function App() {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="/login" element={<LoginPage />} />
          <Route path="/unauthorized" element={<Unauthorized />} />

          <Route element={<ProtectedRoute roles={["PlatformAdmin"]} />}>
            <Route path="/admin" element={<AdminLayout />}>
              <Route index element={<AdminHome />} />
              <Route path="tenants" element={<TenantsPage />} />
              <Route path="org-units" element={<OrgUnitsPage />} />
              <Route path="users" element={<UsersPage />} />
              <Route path="kyc-review" element={<KycReviewPage />} />
              <Route path="transactions" element={<TransactionsPage />} />
              <Route path="audit-trail" element={<AuditTrailPage />} />
              <Route path="reports" element={<ReportsPage />} />
              <Route path="settings" element={<SettingsPage />} />
            </Route>
          </Route>

          <Route element={<ProtectedRoute roles={["OrgUnitAdmin"]} />}>
            <Route path="/org" element={<OrgUnitHome />} />
          </Route>

          <Route element={<ProtectedRoute roles={["EndUser"]} />}>
            <Route path="/me" element={<EndUserHome />} />
          </Route>

          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
}