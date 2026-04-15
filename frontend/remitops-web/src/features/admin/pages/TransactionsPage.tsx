import { useEffect, useMemo, useState } from "react";
import type { CSSProperties } from "react";
import AppButton from "../../../components/ui/AppButton";
import { transactionsApi } from "../api/transactionsApi";
import type { AdminTransactionDto } from "../api/transactionsApi";

type StatusFilter =
  | "all"
  | "New"
  | "InProgress"
  | "Completed"
  | "OnHold"
  | "Rejected"
  | "Cancelled";

type PriorityFilter = "all" | "Critical" | "High" | "Medium" | "Low";
type QueueFilter =
  | "all"
  | "FX"
  | "Payout"
  | "Compliance"
  | "CustomerService"
  | "Screening";

function formatAmount(amount: number, currency: string) {
  return `${currency} ${amount.toLocaleString(undefined, {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  })}`;
}

function formatCompactMoney(amount: number) {
  if (amount >= 1_000_000) return `$${(amount / 1_000_000).toFixed(1)}m`;
  if (amount >= 1_000) return `$${(amount / 1_000).toFixed(1)}k`;
  return `$${amount.toFixed(0)}`;
}

function formatUtc(dateValue: string | null | undefined) {
  if (!dateValue) return "-";
  const date = new Date(dateValue);
  if (Number.isNaN(date.getTime())) return dateValue;
  return date.toLocaleString();
}

function normalizeStatus(status: string | null | undefined) {
  return (status || "Unknown").trim();
}

function normalizePriority(priority: string | null | undefined) {
  return (priority || "Unassigned").trim();
}

function displayStatus(status: string) {
  switch (status) {
    case "InProgress":
      return "In Progress";
    case "OnHold":
      return "On Hold";
    default:
      return status;
  }
}

function displayQueue(queue: string | null | undefined) {
  if (!queue) return "-";
  if (queue === "CustomerService") return "Customer Service";
  return queue;
}

function exportRowsToCsv(rows: AdminTransactionDto[]) {
  const headers = [
    "Reference",
    "Tenant",
    "Beneficiary",
    "Beneficiary Country",
    "Beneficiary City",
    "Amount",
    "Currency",
    "Queue",
    "Status",
    "Priority",
    "Source Org Unit",
    "Destination Org Unit",
    "Submitted By User Id",
    "Last Action By User Id",
    "Created At Utc",
    "Updated At Utc",
    "Transaction Id",
  ];

  const escapeCell = (value: unknown) => {
    const stringValue = value == null ? "" : String(value);
    return `"${stringValue.replace(/"/g, '""')}"`;
  };

  const lines = [
    headers.join(","),
    ...rows.map((row) =>
      [
        row.reference,
        row.tenantName,
        row.beneficiaryName,
        row.beneficiaryCountryCode,
        row.beneficiaryCity,
        row.amount,
        row.currency,
        row.currentQueue,
        row.currentStatus,
        row.priority,
        row.sourceOrgUnitName,
        row.destinationOrgUnitName,
        row.submittedByUserId,
        row.lastActionByUserId,
        row.createdAtUtc,
        row.updatedAtUtc,
        row.id,
      ]
        .map(escapeCell)
        .join(",")
    ),
  ];

  const blob = new Blob([lines.join("\n")], {
    type: "text/csv;charset=utf-8;",
  });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement("a");
  const stamp = new Date().toISOString().slice(0, 19).replace(/[:T]/g, "-");

  anchor.href = url;
  anchor.download = `remitops-transactions-${stamp}.csv`;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);
  URL.revokeObjectURL(url);
}

function statusBadgeClass(status: string) {
  switch (status) {
    case "Completed":
      return "status-badge status-badge--success";
    case "InProgress":
      return "status-badge status-badge--info";
    case "New":
      return "status-badge status-badge--default";
    case "OnHold":
      return "status-badge status-badge--warning";
    case "Rejected":
    case "Cancelled":
      return "status-badge status-badge--danger";
    default:
      return "status-badge status-badge--default";
  }
}

function priorityBadgeStyle(priority: string): CSSProperties {
  switch (priority) {
    case "Critical":
      return {
        background: "#fce7e7",
        color: "#b14141",
        border: "1px solid #f3c8c8",
      };
    case "High":
      return {
        background: "#fff1df",
        color: "#9b6412",
        border: "1px solid #efd4aa",
      };
    case "Medium":
      return {
        background: "#eef4e8",
        color: "#56733d",
        border: "1px solid #d7e3ca",
      };
    case "Low":
      return {
        background: "#f4f5f7",
        color: "#59636f",
        border: "1px solid #d8dde3",
      };
    default:
      return {
        background: "#f4f5f7",
        color: "#59636f",
        border: "1px solid #d8dde3",
      };
  }
}

function stickyColumnStyle(left: number, minWidth: number): CSSProperties {
  return {
    position: "sticky",
    left,
    zIndex: 2,
    background: "#fff",
    minWidth,
    boxShadow: left > 0 ? "1px 0 0 #edf1e8" : undefined,
  };
}

export default function TransactionsPage() {
  const [rows, setRows] = useState<AdminTransactionDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [query, setQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter | "all">("all");
  const [queueFilter, setQueueFilter] = useState<QueueFilter | "all">("all");
  const [priorityFilter, setPriorityFilter] = useState<PriorityFilter | "all">("all");
  const [countryFilter, setCountryFilter] = useState("all");

  async function loadTransactions() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await transactionsApi.list();
      setRows(data);
    } catch (err: any) {
      setError(err.message || "Failed to load transactions.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadTransactions();
  }, []);

  const countryOptions = useMemo(() => {
    return Array.from(
      new Set(
        rows
          .map((r) => (r.beneficiaryCountryCode || "").trim())
          .filter(Boolean)
      )
    ).sort();
  }, [rows]);

  const filteredRows = useMemo(() => {
    const q = query.trim().toLowerCase();

    return rows.filter((row) => {
      const matchesQuery =
        !q ||
        row.reference.toLowerCase().includes(q) ||
        (row.beneficiaryName ?? "").toLowerCase().includes(q) ||
        (row.beneficiaryCity ?? "").toLowerCase().includes(q) ||
        (row.beneficiaryCountryCode ?? "").toLowerCase().includes(q) ||
        row.currentQueue.toLowerCase().includes(q) ||
        row.currentStatus.toLowerCase().includes(q) ||
        row.currency.toLowerCase().includes(q) ||
        row.tenantName.toLowerCase().includes(q) ||
        row.sourceOrgUnitName.toLowerCase().includes(q) ||
        row.destinationOrgUnitName.toLowerCase().includes(q);

      const matchesStatus =
        statusFilter === "all" || normalizeStatus(row.currentStatus) === statusFilter;

      const matchesQueue =
        queueFilter === "all" || (row.currentQueue || "").trim() === queueFilter;

      const matchesPriority =
        priorityFilter === "all" || normalizePriority(row.priority) === priorityFilter;

      const matchesCountry =
        countryFilter === "all" ||
        (row.beneficiaryCountryCode || "").trim() === countryFilter;

      return (
        matchesQuery &&
        matchesStatus &&
        matchesQueue &&
        matchesPriority &&
        matchesCountry
      );
    });
  }, [rows, query, statusFilter, queueFilter, priorityFilter, countryFilter]);

  const totalValue = useMemo(
    () => filteredRows.reduce((sum, row) => sum + row.amount, 0),
    [filteredRows]
  );

  const completedCount = useMemo(
    () =>
      filteredRows.filter(
        (row) => normalizeStatus(row.currentStatus) === "Completed"
      ).length,
    [filteredRows]
  );

  const pendingCount = useMemo(
    () =>
      filteredRows.filter((row) =>
        ["New", "InProgress", "OnHold"].includes(normalizeStatus(row.currentStatus))
      ).length,
    [filteredRows]
  );

  const flaggedCount = useMemo(
    () =>
      filteredRows.filter((row) =>
        ["Rejected", "Cancelled", "OnHold"].includes(
          normalizeStatus(row.currentStatus)
        )
      ).length,
    [filteredRows]
  );

  const latestQueue = useMemo(() => {
    const counts = filteredRows.reduce<Record<string, number>>((acc, row) => {
      const key = row.currentQueue || "Unknown";
      acc[key] = (acc[key] ?? 0) + 1;
      return acc;
    }, {});

    const sorted = Object.entries(counts).sort((a, b) => b[1] - a[1]);
    return sorted[0] ?? ["N/A", 0];
  }, [filteredRows]);

  const activeFilterCount = useMemo(() => {
    return [
      query.trim() !== "",
      statusFilter !== "all",
      queueFilter !== "all",
      priorityFilter !== "all",
      countryFilter !== "all",
    ].filter(Boolean).length;
  }, [query, statusFilter, queueFilter, priorityFilter, countryFilter]);

  return (
    <div>
      <section className="hero">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <p className="hero-eyebrow">Platform Control Center</p>
            <h1 className="hero-title">Transactions</h1>
            <p className="hero-copy">
              Track transaction flow, KYC state, routing queues, and exception handling across corridors.
            </p>
          </div>

          <div
            className="panel-header__action"
            style={{ display: "flex", gap: 10, flexWrap: "wrap" }}
          >
            <AppButton variant="secondary" onClick={loadTransactions}>
              Refresh
            </AppButton>
            <AppButton
              variant="primary"
              onClick={() => exportRowsToCsv(filteredRows)}
              disabled={filteredRows.length === 0}
            >
              Export Transactions
            </AppButton>
          </div>
        </div>
      </section>

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Completed</div>
          <div className="kpi-value">{completedCount}</div>
          <div className="kpi-meta">Settled successfully</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Pending</div>
          <div className="kpi-value">{pendingCount}</div>
          <div className="kpi-meta">Awaiting action</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Flagged</div>
          <div className="kpi-value">{flaggedCount}</div>
          <div className="kpi-meta">Rejected, held, or cancelled</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Filtered value</div>
          <div className="kpi-value">{formatCompactMoney(totalValue)}</div>
          <div className="kpi-meta">Across {filteredRows.length} records</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Top queue</div>
          <div className="kpi-value">{displayQueue(latestQueue[0])}</div>
          <div className="kpi-meta">{latestQueue[1]} transactions</div>
        </div>
      </section>

      <section className="panel-block" style={{ marginBottom: 20 }}>
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <h2>Transaction controls</h2>
            <p className="panel-subtitle">
              Search and filter live transactions, then export the current working set for deeper analysis.
            </p>
          </div>
        </div>

        <div style={filtersGridStyle}>
          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>
              Search
            </div>
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Reference, beneficiary, city, queue, tenant"
              style={inputStyle}
            />
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>
              Status
            </div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as StatusFilter | "all")}
              style={inputStyle}
            >
              <option value="all">All statuses</option>
              <option value="New">New</option>
              <option value="InProgress">In Progress</option>
              <option value="Completed">Completed</option>
              <option value="OnHold">On Hold</option>
              <option value="Rejected">Rejected</option>
              <option value="Cancelled">Cancelled</option>
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>
              Queue
            </div>
            <select
              value={queueFilter}
              onChange={(e) => setQueueFilter(e.target.value as QueueFilter | "all")}
              style={inputStyle}
            >
              <option value="all">All queues</option>
              <option value="FX">FX</option>
              <option value="Payout">Payout</option>
              <option value="Compliance">Compliance</option>
              <option value="CustomerService">Customer Service</option>
              <option value="Screening">Screening</option>
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>
              Priority
            </div>
            <select
              value={priorityFilter}
              onChange={(e) => setPriorityFilter(e.target.value as PriorityFilter | "all")}
              style={inputStyle}
            >
              <option value="all">All priorities</option>
              <option value="Critical">Critical</option>
              <option value="High">High</option>
              <option value="Medium">Medium</option>
              <option value="Low">Low</option>
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>
              Country
            </div>
            <select
              value={countryFilter}
              onChange={(e) => setCountryFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All countries</option>
              {countryOptions.map((country) => (
                <option key={country} value={country}>
                  {country}
                </option>
              ))}
            </select>
          </label>

          <div>
            <div className="cell-muted" style={{ marginBottom: 6 }}>
              Actions
            </div>
            <AppButton
              variant="secondary"
              onClick={() => {
                setQuery("");
                setStatusFilter("all");
                setQueueFilter("all");
                setPriorityFilter("all");
                setCountryFilter("all");
              }}
            >
              Reset
            </AppButton>
          </div>
        </div>

        <div
          style={{
            marginTop: 14,
            display: "flex",
            gap: 10,
            flexWrap: "wrap",
            alignItems: "center",
          }}
        >
          <span className="cell-muted">
            {filteredRows.length} matching transactions
          </span>
          <span className="cell-muted">•</span>
          <span className="cell-muted">
            {activeFilterCount} active filter{activeFilterCount === 1 ? "" : "s"}
          </span>
          <span className="cell-muted">•</span>
          <span className="cell-muted">
            Export respects current filters
          </span>
        </div>

        {isLoading && (
          <div className="loading-state" style={{ marginTop: 14 }}>
            Loading transactions…
          </div>
        )}

        {error && !isLoading && (
          <div className="alert alert-danger" style={{ marginTop: 14 }}>
            {error}
          </div>
        )}
      </section>

      <section className="panel-block">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <h2>Live transactions</h2>
            <div className="cell-muted">
              {filteredRows.length} records
            </div>
          </div>
          <div className="cell-muted">
            Status and priority pinned first for faster triage
          </div>
        </div>

        <div className="table-shell table-wide">
          <table className="data-table">
            <thead>
              <tr>
                <th
                  className="cell-mono"
                  style={{ ...stickyHeaderStyle, ...stickyColumnStyle(0, 150), zIndex: 5 }}
                >
                  Reference
                </th>
                <th
                  style={{ ...stickyHeaderStyle, ...stickyColumnStyle(150, 130), zIndex: 5 }}
                >
                  Status
                </th>
                <th
                  style={{ ...stickyHeaderStyle, ...stickyColumnStyle(280, 120), zIndex: 5 }}
                >
                  Priority
                </th>
                <th>Beneficiary</th>
                <th>Source</th>
                <th>Destination</th>
                <th>Amount</th>
                <th>Queue</th>
                <th>Country</th>
                <th>Created</th>
              </tr>
            </thead>

            <tbody>
              {!isLoading && filteredRows.length === 0 ? (
                <tr>
                  <td colSpan={10} className="cell-muted">
                    No transactions match your current filters.
                  </td>
                </tr>
              ) : (
                filteredRows.map((row) => {
                  const priority = normalizePriority(row.priority);
                  const status = normalizeStatus(row.currentStatus);

                  return (
                    <tr key={row.id}>
                      <td
                        className="cell-mono"
                        style={{ ...stickyBodyCellStyle, ...stickyColumnStyle(0, 150), zIndex: 4 }}
                      >
                        {row.reference}
                      </td>

                      <td
                        style={{ ...stickyBodyCellStyle, ...stickyColumnStyle(150, 130), zIndex: 4 }}
                      >
                        <span className={statusBadgeClass(status)}>
                          {displayStatus(status)}
                        </span>
                      </td>

                      <td
                        style={{ ...stickyBodyCellStyle, ...stickyColumnStyle(280, 120), zIndex: 4 }}
                      >
                        <span
                          style={{
                            ...priorityBadgeStyle(priority),
                            display: "inline-flex",
                            alignItems: "center",
                            minHeight: 28,
                            borderRadius: 999,
                            padding: "0 10px",
                            fontSize: 12,
                            fontWeight: 700,
                            whiteSpace: "nowrap",
                          }}
                        >
                          {priority}
                        </span>
                      </td>

                      <td>
                        <div style={{ fontWeight: 700 }}>{row.beneficiaryName ?? "-"}</div>
                        <div className="cell-muted">{row.beneficiaryCity ?? "-"}</div>
                      </td>
                      <td>{row.sourceOrgUnitName}</td>
                      <td>{row.destinationOrgUnitName}</td>
                      <td className="cell-mono">{formatAmount(row.amount, row.currency)}</td>
                      <td>{displayQueue(row.currentQueue)}</td>
                      <td>{row.beneficiaryCountryCode ?? "-"}</td>
                      <td className="cell-mono">{formatUtc(row.createdAtUtc)}</td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
  );
}

const inputStyle: CSSProperties = {
  width: "100%",
  minHeight: 44,
  borderRadius: 12,
  border: "1px solid #dbe5d6",
  background: "#fff",
  padding: "0 14px",
  font: "inherit",
  color: "#243126",
  outline: "none",
};

const filtersGridStyle: CSSProperties = {
  display: "grid",
  gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
  gap: 12,
  alignItems: "end",
};

const stickyHeaderStyle: CSSProperties = {
  position: "sticky",
  top: 0,
  background: "#f7faf4",
};

const stickyBodyCellStyle: CSSProperties = {
  background: "#fff",
};