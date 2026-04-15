import { useEffect, useMemo, useState } from "react";
import {
  fetchDashboardSummary,
  type DashboardSummaryDto,
} from "../features/admin/api/dashboardApi";
import { tenantsApi } from "../features/admin/api/tenantsApi";
import type { TenantDto } from "../features/admin/api/tenantsApi";
import TenantGeoMap from "../features/admin/components/TenantGeoMap";
import DailyVolumeChart, {
  type DailyVolumePoint,
} from "../features/admin/components/DailyVolumeChart";

function money(value: number) {
  return new Intl.NumberFormat(undefined, {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: 0,
  }).format(value);
}

export default function AdminHome() {
  const [summary, setSummary] = useState<DashboardSummaryDto | null>(null);
  const [tenants, setTenants] = useState<TenantDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;

    async function load() {
      setLoading(true);
      setError("");

      try {
        const [summaryData, tenantsData] = await Promise.all([
          fetchDashboardSummary(),
          tenantsApi.list(),
        ]);

        if (!cancelled) {
          setSummary(summaryData);
          setTenants(tenantsData);
        }
      } catch (err: any) {
        if (!cancelled) {
          setError(err.message || "Failed to load dashboard.");
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    load();

    return () => {
      cancelled = true;
    };
  }, []);

  const dailyVolumeData: DailyVolumePoint[] = useMemo(() => {
    return (summary?.dailyRemittanceVolume ?? []).map((item) => ({
      date: item.date,
      volume: item.totalAmount,
    }));
  }, [summary]);

  if (loading) {
    return <div className="panel-block">Loading dashboard…</div>;
  }

  if (error) {
    return <div className="panel-block">{error}</div>;
  }

  return (
    <div>
      <section className="hero">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <p className="hero-eyebrow">Platform Control Center</p>
            <h1 className="hero-title">Platform Dashboard</h1>
            <p className="hero-copy">
              Monitor platform growth, operations, and geographic footprint.
            </p>
          </div>
        </div>
      </section>

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Tenants</div>
          <div className="kpi-value">{summary?.totalTenants ?? 0}</div>
          <div className="kpi-meta">Across corridors</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Org units</div>
          <div className="kpi-value">{summary?.totalOrgUnits ?? 0}</div>
          <div className="kpi-meta">Branches and hubs</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Active users</div>
          <div className="kpi-value">{summary?.totalUsers ?? 0}</div>
          <div className="kpi-meta">Platform identities</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Remittance volume</div>
          <div className="kpi-value">
            {money(summary?.totalRemittanceAmount ?? 0)}
          </div>
          <div className="kpi-meta">
            {summary?.totalRemittanceRequests ?? 0} requests
          </div>
        </div>
      </section>

      <section
        className="panel-block"
        style={{
          marginTop: 20,
          display: "grid",
          gridTemplateColumns: "1.3fr 1fr",
          gap: 20,
        }}
      >
        <div>
          <div className="panel-header">
            <h2>Tenant geographic distribution</h2>
          </div>
          <TenantGeoMap tenants={tenants} />
        </div>

        <div>
          <div className="panel-header">
            <h2>Daily remittance volume</h2>
          </div>
          <DailyVolumeChart data={dailyVolumeData} />
        </div>
      </section>

      <section
        style={{
          display: "grid",
          gridTemplateColumns: "repeat(3, minmax(0, 1fr))",
          gap: 20,
          marginTop: 20,
        }}
      >
        <div className="panel-block">
          <div className="panel-header">
            <h2>Compliance status</h2>
          </div>
          <div className="kpi-value">
            {summary?.totalRemittanceRequests ?? 0}
          </div>
          <div className="kpi-meta">Total remittance requests</div>
        </div>

        <div className="panel-block">
          <div className="panel-header">
            <h2>Flagged transactions</h2>
          </div>
          <div className="kpi-value">0</div>
          <div className="kpi-meta">Placeholder (TBD)</div>
        </div>

        <div className="panel-block">
          <div className="panel-header">
            <h2>Coverage</h2>
          </div>
          <div className="kpi-value">
            {tenants.filter((t) => t.countryCode).length}
          </div>
          <div className="kpi-meta">Tenants with mapped geo data</div>
        </div>
      </section>
    </div>
  );
}