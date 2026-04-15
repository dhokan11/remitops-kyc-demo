import PageHeader from "../../../components/ui/PageHeader";
import DataTable from "../../../components/ui/DataTable";
import StatusBadge, { getStatusTone } from "../../../components/ui/StatusBadge";
import AppButton from "../../../components/ui/AppButton";

const rows = [
  {
    id: "USR-001",
    name: "Platform Admin",
    email: "platform.admin@remitops.local",
    role: "PlatformAdmin",
    status: "Active",
  },
  {
    id: "USR-002",
    name: "Org Admin 01",
    email: "orgadmin01@remitops.local",
    role: "OrgUnitAdmin",
    status: "Active",
  },
  {
    id: "USR-003",
    name: "End User 01",
    email: "enduser01@remitops.local",
    role: "EndUser",
    status: "Pending",
  },
];

export default function UsersPage() {
  return (
    <>
      <PageHeader
        title="Users"
        subtitle="Manage platform, branch, and end-user access across the system."
        action={<AppButton variant="primary">Invite User</AppButton>}
      />

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Total users</div>
          <div className="kpi-value">66</div>
          <div className="kpi-meta">Across all roles</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Platform admins</div>
          <div className="kpi-value">1</div>
          <div className="kpi-meta">Global control</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Org unit admins</div>
          <div className="kpi-value">15</div>
          <div className="kpi-meta">Branch operations</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">Pending activation</div>
          <div className="kpi-value text-warning">3</div>
          <div className="kpi-meta">Require follow-up</div>
        </div>
      </section>

      <div className="card">
        <DataTable
          rows={rows}
          columns={[
            { key: "id", header: "User ID", className: "cell-mono cell-muted" },
            { key: "name", header: "Name" },
            { key: "email", header: "Email" },
            { key: "role", header: "Role" },
            {
              key: "status",
              header: "Status",
              render: (value) => (
                <StatusBadge tone={getStatusTone(String(value))}>
                  {String(value)}
                </StatusBadge>
              ),
            },
          ]}
        />
      </div>
    </>
  );
}