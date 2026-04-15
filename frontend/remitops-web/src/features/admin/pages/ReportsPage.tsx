import PageHeader from "../../../components/ui/PageHeader";

export default function ReportsPage() {
  return (
    <>
      <PageHeader
        title="Reports"
        subtitle="Monitor corridor performance, transaction health, and operational trends."
        action={<button className="btn btn-secondary">Export Summary</button>}
      />

      <div className="content-grid">
        <div className="card panel-block">
          <div className="panel-header">
            <h2>Performance overview</h2>
          </div>

          <div className="queue-list">
            <div className="queue-item">
              <span>Monthly transaction volume</span>
              <strong>1,284</strong>
            </div>
            <div className="queue-item">
              <span>Average approval time</span>
              <strong>18 min</strong>
            </div>
            <div className="queue-item">
              <span>Top corridor</span>
              <strong>SO → KE</strong>
            </div>
            <div className="queue-item">
              <span>Compliance pass rate</span>
              <strong>96%</strong>
            </div>
          </div>
        </div>

        <div className="card panel-block">
          <div className="panel-header">
            <h2>Exports</h2>
          </div>

          <div className="queue-list">
            <div className="queue-item">
              <span>Tenant summary</span>
              <strong>CSV</strong>
            </div>
            <div className="queue-item">
              <span>KYC review log</span>
              <strong>XLSX</strong>
            </div>
            <div className="queue-item">
              <span>Audit trail extract</span>
              <strong>JSON</strong>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}