import PageHeader from "../../../components/ui/PageHeader";

export default function SettingsPage() {
  return (
    <>
      <PageHeader
        title="Settings"
        subtitle="Configure system defaults, operational preferences, and admin controls."
        action={<button className="btn btn-primary">Save Changes</button>}
      />

      <div className="content-grid">
        <div className="card panel-block">
          <div className="panel-header">
            <h2>Platform defaults</h2>
          </div>

          <div className="form-inline" style={{ gridTemplateColumns: "1fr 1fr" }}>
            <div className="form-group">
              <label>Default currency</label>
              <input value="USD" readOnly />
            </div>

            <div className="form-group">
              <label>Default time zone</label>
              <input value="Africa/Hargeisa" readOnly />
            </div>

            <div className="form-group">
              <label>KYC review SLA</label>
              <input value="24 hours" readOnly />
            </div>

            <div className="form-group">
              <label>Brand profile</label>
              <input value="Dahabshiil-inspired" readOnly />
            </div>
          </div>
        </div>

        <div className="card panel-block">
          <div className="panel-header">
            <h2>Security</h2>
          </div>

          <div className="queue-list">
            <div className="queue-item">
              <span>Session timeout</span>
              <strong>30 min</strong>
            </div>
            <div className="queue-item">
              <span>Password policy</span>
              <strong>Enabled</strong>
            </div>
            <div className="queue-item">
              <span>Audit logging</span>
              <strong>Active</strong>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}