import { useEffect, useMemo, useState } from "react";
import AppButton from "../../../components/ui/AppButton";
import { dashboardApi } from "../api/dashboardApi";
import type {
  DashboardDailyVolumePointDto,
  DashboardSummaryDto,
} from "../api/dashboardApi";

function formatCompactMoney(amount: number) {
  if (amount >= 1_000_000) return `$${(amount / 1_000_000).toFixed(1)}m`;
  if (amount >= 1_000) return `$${(amount / 1_000).toFixed(1)}k`;
  return `$${amount.toFixed(0)}`;
}

function formatAxisLabel(dateValue: string) {
  const date = new Date(dateValue);
  if (Number.isNaN(date.getTime())) return dateValue;
  return date.toLocaleDateString(undefined, { month: "short", day: "numeric" });
}

function getChartGeometry(points: DashboardDailyVolumePointDto[]) {
  const width = 760;
  const height = 220;
  const paddingX = 18;
  const paddingTop = 18;
  const paddingBottom = 28;

  if (points.length === 0) {
    return {
      width,
      height,
      path: "",
      areaPath: "",
      guideLines: [],
      plotted: [] as Array<{ x: number; y: number; point: DashboardDailyVolumePointDto }>,
      maxValue: 0,
    };
  }

  const maxValue = Math.max(...points.map((p) => p.totalAmount), 1);
  const chartWidth = width - paddingX * 2;
  const chartHeight = height - paddingTop - paddingBottom;

  const plotted = points.map((point, index) => {
    const x =
      points.length === 1
        ? width / 2
        : paddingX + (index / (points.length - 1)) * chartWidth;

    const y =
      paddingTop +
      chartHeight -
      (point.totalAmount / maxValue) * chartHeight;

    return { x, y, point };
  });

  const path = plotted
    .map((p, index) => `${index === 0 ? "M" : "L"} ${p.x} ${p.y}`)
    .join(" ");

  const areaPath = plotted.length
    ? `${path} L ${plotted[plotted.length - 1].x} ${height - paddingBottom} L ${plotted[0].x} ${height - paddingBottom} Z`
    : "";

  const guideLines = [0.25, 0.5, 0.75].map((ratio) => ({
    y: paddingTop + chartHeight - chartHeight * ratio,
  }));

  return {
    width,
    height,
    path,
    areaPath,
    guideLines,
    plotted,
    maxValue,
  };
}

export default function OverviewPage() {
  const [summary, setSummary] = useState<DashboardSummaryDto | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  async function loadDashboard() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await dashboardApi.getSummary();
      setSummary(data);
    } catch (err: any) {
      setError(err.message || "Failed to load dashboard summary.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadDashboard();
  }, []);

  const chartPoints = summary?.dailyRemittanceVolume ?? [];
  const chart = useMemo(() => getChartGeometry(chartPoints), [chartPoints]);

  const latestDay = chartPoints[chartPoints.length - 1];
  const previousDay = chartPoints[chartPoints.length - 2];

  const latestDeltaText = useMemo(() => {
    if (!latestDay) return "No recent transaction data";
    if (!previousDay) return `${latestDay.transactionCount} transactions on latest day`;

    const delta = latestDay.totalAmount - previousDay.totalAmount;
    const direction = delta > 0 ? "up" : delta < 0 ? "down" : "flat";
    const absolute = Math.abs(delta);

    if (direction === "flat") {
      return `Flat versus previous day`;
    }

    return `${direction} ${formatCompactMoney(absolute)} vs previous day`;
  }, [latestDay, previousDay]);

  return (
    <div>
      <section className="hero">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <p className="hero-eyebrow">Platform Control Center</p>
            <h1 className="hero-title">Overview</h1>
            <p className="hero-copy">
              Monitor live operating scale, user base, and remittance movement across the platform.
            </p>
          </div>

          <div className="panel-header__action">
            <AppButton variant="secondary" onClick={loadDashboard}>
              Refresh Dashboard
            </AppButton>
          </div>
        </div>
      </section>

      {error && !isLoading && (
        <section className="panel-block" style={{ marginBottom: 20 }}>
          <div className="alert alert-danger">{error}</div>
        </section>
      )}

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Tenants</div>
          <div className="kpi-value">{summary?.totalTenants ?? (isLoading ? "…" : 0)}</div>
          <div className="kpi-meta">Active platform entities</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Org units</div>
          <div className="kpi-value">{summary?.totalOrgUnits ?? (isLoading ? "…" : 0)}</div>
          <div className="kpi-meta">Operational branches</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Users</div>
          <div className="kpi-value">{summary?.totalUsers ?? (isLoading ? "…" : 0)}</div>
          <div className="kpi-meta">Provisioned accounts</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Transactions</div>
          <div className="kpi-value">
            {summary?.totalRemittanceRequests ?? (isLoading ? "…" : 0)}
          </div>
          <div className="kpi-meta">Remittance requests recorded</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Total remittance value</div>
          <div className="kpi-value">
            {isLoading
              ? "…"
              : formatCompactMoney(summary?.totalRemittanceAmount ?? 0)}
          </div>
          <div className="kpi-meta">From live transactions database</div>
        </div>
      </section>

      <section className="panel-block">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <h2>Daily remittance volume</h2>
            <p className="panel-subtitle">
              Last 14 days of live transaction volume aggregated from remittance requests.
            </p>
          </div>
          <div className="cell-muted">{latestDeltaText}</div>
        </div>

        {isLoading ? (
          <div className="loading-state">Loading live volume chart…</div>
        ) : chartPoints.length === 0 ? (
          <div className="loading-state">No remittance volume data available.</div>
        ) : (
          <>
            <div
              style={{
                width: "100%",
                overflowX: "auto",
                borderRadius: 18,
                background: "linear-gradient(180deg, #f7fbf4 0%, #ffffff 100%)",
                border: "1px solid #e2ebdc",
                padding: 16,
              }}
            >
              <svg
                viewBox={`0 0 ${chart.width} ${chart.height}`}
                style={{ width: "100%", minWidth: 680, height: 240, display: "block" }}
                role="img"
                aria-label="Daily remittance volume chart"
              >
                <defs>
                  <linearGradient id="overviewVolumeArea" x1="0" x2="0" y1="0" y2="1">
                    <stop offset="0%" stopColor="#4f8a41" stopOpacity="0.26" />
                    <stop offset="100%" stopColor="#4f8a41" stopOpacity="0.03" />
                  </linearGradient>
                </defs>

                {chart.guideLines.map((line, index) => (
                  <line
                    key={index}
                    x1="18"
                    x2={chart.width - 18}
                    y1={line.y}
                    y2={line.y}
                    stroke="#dfe8d9"
                    strokeDasharray="4 6"
                  />
                ))}

                {chart.areaPath && (
                  <path d={chart.areaPath} fill="url(#overviewVolumeArea)" />
                )}

                {chart.path && (
                  <path
                    d={chart.path}
                    fill="none"
                    stroke="#4f8a41"
                    strokeWidth="3.5"
                    strokeLinecap="round"
                    strokeLinejoin="round"
                  />
                )}

                {chart.plotted.map(({ x, y, point }) => (
                  <g key={point.date}>
                    <circle cx={x} cy={y} r="4.5" fill="#4f8a41" />
                  </g>
                ))}
              </svg>
            </div>

            <div
              style={{
                marginTop: 14,
                display: "grid",
                gridTemplateColumns: "repeat(auto-fit, minmax(120px, 1fr))",
                gap: 10,
              }}
            >
              {chartPoints.map((point) => (
                <div
                  key={point.date}
                  style={{
                    borderRadius: 14,
                    border: "1px solid #e2ebdc",
                    background: "#fbfdf9",
                    padding: "12px 14px",
                  }}
                >
                  <div className="cell-muted" style={{ marginBottom: 4 }}>
                    {formatAxisLabel(point.date)}
                  </div>
                  <div style={{ fontWeight: 800, color: "#264a22" }}>
                    {formatCompactMoney(point.totalAmount)}
                  </div>
                  <div className="cell-muted">
                    {point.transactionCount} txns
                  </div>
                </div>
              ))}
            </div>
          </>
        )}
      </section>
    </div>
  );
}