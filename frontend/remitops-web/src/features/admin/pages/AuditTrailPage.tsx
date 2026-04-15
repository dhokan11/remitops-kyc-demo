import PageHeader from "../../../components/ui/PageHeader";
import DataTable from "../../../components/ui/DataTable";
import StatusBadge, { getStatusTone } from "../../../components/ui/StatusBadge";
import AppButton from "../../../components/ui/AppButton";

const rows = [
  {
    id: "5001",
    actor: "platform.admin@remitops.local",
    action: "LOGIN",
    entityType: "AUTH",
    entityId: "seed-platform-admin",
    createdAtUtc: "2026-04-14 09:10 UTC",
  },
  {
    id: "5002",
    actor: "platform.admin@remitops.local",
    action: "CREATE",
    entityType: "TENANT",
    entityId: "TEN-006",
    createdAtUtc: "2026-04-14 09:15 UTC",
  },
  {
    id: "5003",
    actor: "platform.admin@remitops.local",
    action: "VIEWAUDIT",
    entityType: "AUDIT",
    entityId: "0",
    createdAtUtc: "2026-04-14 09:20 UTC",
  },
];

export default function AuditTrailPage() {
  return (
    <>
      <PageHeader
        title="Audit Trail"
        subtitle="Track system access, admin actions, and entity-level operational history."
        action={
          <div className="hero-actions">
            <AppButton variant="secondary">Refresh</AppButton>
            <AppButton variant="primary">Export Audit</AppButton>
          </div>
        }
      />

      <div className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Events today</div>
          <div className="kpi-value">128</div>
          <div className="kpi-meta">Across admin actions</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Logins</div>
          <div className="kpi-value text-success">41</div>
          <div className="kpi-meta">Authenticated access</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Entity changes</div>
          <div className="kpi-value">63</div>
          <div className="kpi-meta">Create and update events</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Risk events</div>
          <div className="kpi-value text-warning">5</div>
          <div className="kpi-meta">Require review</div>
        </div>
      </div>

      <div className="card">
        <DataTable
          rows={rows}
          columns={[
            { key: "id", header: "Event ID", className: "cell-mono cell-muted" },
            { key: "actor", header: "Actor" },
            {
              key: "action",
              header: "Action",
              render: (value) => (
                <StatusBadge tone={getStatusTone(String(value))}>{String(value)}</StatusBadge>
              ),
            },
            { key: "entityType", header: "Entity Type" },
            { key: "entityId", header: "Entity ID", className: "cell-mono" },
            { key: "createdAtUtc", header: "Created UTC" },
          ]}
        />
      </div>
    </>
  );
}