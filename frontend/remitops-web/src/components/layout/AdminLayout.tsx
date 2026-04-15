import { Outlet, useLocation, useNavigate } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import AppSidebar from "./AppSidebar";
import AppButton from "../ui/AppButton";

function formatPathname(pathname: string) {
  const clean = pathname.replace(/^\/admin/, "") || "/overview";
  const normalized = clean === "/" ? "overview" : clean.replace(/^\//, "");

  return normalized
    .split("/")
    .filter(Boolean)
    .join(" / ")
    .replace(/-/g, " ")
    .replace(/\b\w/g, (m) => m.toUpperCase());
}

export default function AdminLayout() {
  const location = useLocation();
  const navigate = useNavigate();
  const [mobileSidebarOpen, setMobileSidebarOpen] = useState(false);

  useEffect(() => {
    setMobileSidebarOpen(false);
  }, [location.pathname]);

  const title = useMemo(() => formatPathname(location.pathname), [location.pathname]);

  function handleLogout() {
    localStorage.removeItem("auth_user");
    navigate("/login", { replace: true });
  }

  return (
    <div className={`admin-shell ${mobileSidebarOpen ? "sidebar-open" : ""}`}>
      <AppSidebar
        mobileOpen={mobileSidebarOpen}
        onClose={() => setMobileSidebarOpen(false)}
        onLogout={handleLogout}
      />

      <main className="admin-main">
        <header className="topbar">
          <div className="topbar__left">
            <button
              className="sidebar-toggle"
              type="button"
              onClick={() => setMobileSidebarOpen((v) => !v)}
              aria-label="Toggle navigation"
              aria-expanded={mobileSidebarOpen}
              aria-controls="admin-sidebar"
            >
              ☰
            </button>

            <div className="topbar__title">{title}</div>
          </div>

          <div className="topbar__actions">
            <div className="topbar__user">platform.admin@remitops.local</div>
            <AppButton variant="secondary" onClick={handleLogout}>
              Logout
            </AppButton>
          </div>
        </header>

        <section className="page-body">
          <Outlet />
        </section>
      </main>

      {mobileSidebarOpen ? (
        <button
          type="button"
          className="sidebar-backdrop"
          aria-label="Close navigation"
          onClick={() => setMobileSidebarOpen(false)}
        />
      ) : null}
    </div>
  );
}