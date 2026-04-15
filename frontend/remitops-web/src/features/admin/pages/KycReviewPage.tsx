import { useEffect, useMemo, useState } from "react";
import PageHeader from "../../../components/ui/PageHeader";
import AppButton from "../../../components/ui/AppButton";
import StatusBadge, { getStatusTone } from "../../../components/ui/StatusBadge";
import { kycApi } from "../api/kycApi";
import type { AdminKycReviewDto } from "../api/kycApi";

type StatusFilter = "all" | "Pending" | "Approved" | "Rejected" | "Flagged";

function normalize(value: string | null | undefined, fallback = "-") {
  const text = (value || "").trim();
  return text || fallback;
}

function formatUtc(value: string | null | undefined) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return date.toLocaleString();
}

function formatDocumentType(value: string | null | undefined) {
  const text = normalize(value, "Unknown");
  return text
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/_/g, " ");
}

function formatReference(id: string) {
  return `KYC-${id.slice(0, 8).toUpperCase()}`;
}

function exportRowsToCsv(rows: AdminKycReviewDto[]) {
  const headers = [
    "Reference",
    "KYC Id",
    "Tenant Name",
    "Org Unit Name",
    "Identity User Id",
    "Applicant Name",
    "Applicant Email",
    "Document Type",
    "File Name",
    "Status",
    "Submitted At Utc",
  ];

  const escapeCell = (value: unknown) => {
    const stringValue = value == null ? "" : String(value);
    return `"${stringValue.replace(/"/g, '""')}"`;
  };

  const lines = [
    headers.join(","),
    ...rows.map((row) =>
      [
        formatReference(row.id),
        row.id,
        row.tenantName,
        row.orgUnitName,
        row.identityUserId,
        row.applicantName,
        row.applicantEmail,
        row.documentType,
        row.fileName,
        row.status,
        row.submittedAtUtc ?? "",
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
  anchor.download = `remitops-kyc-review-${stamp}.csv`;
  document.body.appendChild(anchor);
  anchor.click();
  document.body.removeChild(anchor);
  URL.revokeObjectURL(url);
}

export default function KycReviewPage() {
  const [rows, setRows] = useState<AdminKycReviewDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [query, setQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<StatusFilter>("all");
  const [documentTypeFilter, setDocumentTypeFilter] = useState("all");
  const [orgUnitFilter, setOrgUnitFilter] = useState("all");

  async function loadKycCases() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await kycApi.list();
      setRows(data);
    } catch (err: any) {
      setError(err?.message || "Failed to load KYC review queue.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadKycCases();
  }, []);

  const documentTypeOptions = useMemo(() => {
    return Array.from(
      new Set(rows.map((r) => normalize(r.documentType, "")).filter(Boolean))
    ).sort();
  }, [rows]);

  const orgUnitOptions = useMemo(() => {
    return Array.from(
      new Set(rows.map((r) => normalize(r.orgUnitName, "")).filter(Boolean))
    ).sort();
  }, [rows]);

  const filteredRows = useMemo(() => {
    const q = query.trim().toLowerCase();

    return rows.filter((row) => {
      const matchesQuery =
        !q ||
        formatReference(row.id).toLowerCase().includes(q) ||
        normalize(row.identityUserId, "").toLowerCase().includes(q) ||
        normalize(row.applicantName, "").toLowerCase().includes(q) ||
        normalize(row.applicantEmail, "").toLowerCase().includes(q) ||
        normalize(row.orgUnitName, "").toLowerCase().includes(q) ||
        normalize(row.tenantName, "").toLowerCase().includes(q) ||
        normalize(row.documentType, "").toLowerCase().includes(q) ||
        normalize(row.fileName, "").toLowerCase().includes(q) ||
        normalize(row.status, "").toLowerCase().includes(q);

      const matchesStatus =
        statusFilter === "all" || normalize(row.status, "") === statusFilter;

      const matchesDocumentType =
        documentTypeFilter === "all" ||
        normalize(row.documentType, "") === documentTypeFilter;

      const matchesOrgUnit =
        orgUnitFilter === "all" || normalize(row.orgUnitName, "") === orgUnitFilter;

      return matchesQuery && matchesStatus && matchesDocumentType && matchesOrgUnit;
    });
  }, [rows, query, statusFilter, documentTypeFilter, orgUnitFilter]);

  const pendingCount = useMemo(
    () => filteredRows.filter((x) => normalize(x.status, "") === "Pending").length,
    [filteredRows]
  );

  const approvedCount = useMemo(
    () => filteredRows.filter((x) => normalize(x.status, "") === "Approved").length,
    [filteredRows]
  );

  const rejectedCount = useMemo(
    () => filteredRows.filter((x) => normalize(x.status, "") === "Rejected").length,
    [filteredRows]
  );

  const flaggedCount = useMemo(
    () =>
      filteredRows.filter((x) =>
        ["Flagged", "Pending Review", "Needs Review"].includes(normalize(x.status, ""))
      ).length,
    [filteredRows]
  );

  const activeFilterCount = useMemo(() => {
    return [
      query.trim() !== "",
      statusFilter !== "all",
      documentTypeFilter !== "all",
      orgUnitFilter !== "all",
    ].filter(Boolean).length;
  }, [query, statusFilter, documentTypeFilter, orgUnitFilter]);

  return (
    <>
      <PageHeader
        title="KYC Review"
        subtitle="Review identity documents and compliance cases from the live queue."
        action={
          <div style={{ display: "flex", gap: 10, flexWrap: "wrap" }}>
            <AppButton variant="secondary" onClick={loadKycCases}>
              Refresh
            </AppButton>
            <AppButton variant="warning" onClick={() => setStatusFilter("Pending")}>
              Open Priority Queue
            </AppButton>
            <AppButton
              variant="primary"
              onClick={() => exportRowsToCsv(filteredRows)}
              disabled={filteredRows.length === 0}
            >
              Export Cases
            </AppButton>
          </div>
        }
      />

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Pending review</div>
          <div className="kpi-value text-warning">{pendingCount}</div>
          <div className="kpi-meta">Awaiting decision</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Approved</div>
          <div className="kpi-value text-success">{approvedCount}</div>
          <div className="kpi-meta">Cleared in current result set</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Rejected</div>
          <div className="kpi-value text-danger">{rejectedCount}</div>
          <div className="kpi-meta">Closed with rejection</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Needs attention</div>
          <div className="kpi-value">{flaggedCount}</div>
          <div className="kpi-meta">Manual follow-up likely</div>
        </div>
      </section>

      <section className="panel-block" style={{ marginBottom: 20 }}>
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <h2>KYC controls</h2>
            <p className="panel-subtitle">
              Filter the live KYC queue by applicant, document type, status, tenant, and org unit.
            </p>
          </div>
        </div>

        <div style={filtersGridStyle}>
          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Search</div>
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Reference, applicant, tenant, org unit, document"
              style={inputStyle}
            />
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Status</div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as StatusFilter)}
              style={inputStyle}
            >
              <option value="all">All statuses</option>
              <option value="Pending">Pending</option>
              <option value="Approved">Approved</option>
              <option value="Rejected">Rejected</option>
              <option value="Flagged">Flagged</option>
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Document type</div>
            <select
              value={documentTypeFilter}
              onChange={(e) => setDocumentTypeFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All document types</option>
              {documentTypeOptions.map((item) => (
                <option key={item} value={item}>
                  {formatDocumentType(item)}
                </option>
              ))}
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Org unit</div>
            <select
              value={orgUnitFilter}
              onChange={(e) => setOrgUnitFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All org units</option>
              {orgUnitOptions.map((item) => (
                <option key={item} value={item}>
                  {item}
                </option>
              ))}
            </select>
          </label>
        </div>

        <div
          style={{
            marginTop: 12,
            display: "flex",
            gap: 10,
            flexWrap: "wrap",
            alignItems: "center",
          }}
        >
          <AppButton
            variant="secondary"
            onClick={() => {
              setQuery("");
              setStatusFilter("all");
              setDocumentTypeFilter("all");
              setOrgUnitFilter("all");
            }}
          >
            Reset filters
          </AppButton>

          <span className="cell-muted">{filteredRows.length} matching cases</span>
          <span className="cell-muted">•</span>
          <span className="cell-muted">
            {activeFilterCount} active filter{activeFilterCount === 1 ? "" : "s"}
          </span>
          <span className="cell-muted">•</span>
          <span className="cell-muted">Export respects current filters</span>
        </div>

        {isLoading && (
          <div className="loading-state" style={{ marginTop: 14 }}>
            Loading KYC queue…
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
            <h2>Live KYC queue</h2>
            <div className="cell-muted">{filteredRows.length} records</div>
          </div>
          <div className="cell-muted">Status-first layout for faster review</div>
        </div>

        <div className="table-shell table-wide">
          <table className="data-table">
            <thead>
              <tr>
                <th className="cell-mono">Reference</th>
                <th>Status</th>
                <th>Applicant</th>
                <th>Document</th>
                <th>Org Unit</th>
                <th>Tenant</th>
                <th>Submitted</th>
              </tr>
            </thead>
            <tbody>
              {!isLoading && filteredRows.length === 0 ? (
                <tr>
                  <td colSpan={7} className="cell-muted">
                    No KYC cases match your current filters.
                  </td>
                </tr>
              ) : (
                filteredRows.map((row) => {
                  const status = normalize(row.status, "Unknown");

                  return (
                    <tr key={row.id}>
                      <td className="cell-mono">{formatReference(row.id)}</td>
                      <td>
                        <StatusBadge tone={getStatusTone(status)}>
                          {status}
                        </StatusBadge>
                      </td>
                      <td>
                        <div style={{ fontWeight: 700 }}>
                          {normalize(row.applicantName, row.identityUserId)}
                        </div>
                        <div className="cell-muted">{normalize(row.applicantEmail)}</div>
                      </td>
                      <td>
                        <div style={{ fontWeight: 700 }}>
                          {formatDocumentType(row.documentType)}
                        </div>
                        <div className="cell-muted">{normalize(row.fileName)}</div>
                      </td>
                      <td>{normalize(row.orgUnitName)}</td>
                      <td>{normalize(row.tenantName)}</td>
                      <td className="cell-mono">{formatUtc(row.submittedAtUtc)}</td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </section>
    </>
  );
}

const inputStyle: React.CSSProperties = {
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

const filtersGridStyle: React.CSSProperties = {
  display: "grid",
  gridTemplateColumns: "repeat(auto-fit, minmax(180px, 1fr))",
  gap: 12,
  alignItems: "end",
};