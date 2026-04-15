import { FormEvent, useEffect, useMemo, useState } from "react";
import AppButton from "../../../components/ui/AppButton";

type TenantOption = {
  id: string;
  code: string;
  name: string;
};

type OrgUnitRecord = {
  id: number;
  tenantId: string;
  code: string;
  name: string;
  city: string;
  countryCode: string;
  latitude: string;
  longitude: string;
  status: "Active" | "Inactive";
};

type OrgUnitFormState = {
  tenantId: string;
  code: string;
  name: string;
  city: string;
  countryCode: string;
  latitude: string;
  longitude: string;
  status: "Active" | "Inactive";
};

const tenantOptions: TenantOption[] = [
  { id: "TEN-001", code: "TEN-001", name: "Dahab Hargeisa" },
  { id: "TEN-002", code: "TEN-002", name: "Dahab Mogadishu" },
  { id: "TEN-003", code: "TEN-003", name: "Dahab Nairobi" },
  { id: "TEN-004", code: "TEN-004", name: "Dahab Dubai" },
  { id: "TEN-005", code: "TEN-005", name: "Dahab London" },
];

const initialRows: OrgUnitRecord[] = [
  {
    id: 1,
    tenantId: "TEN-001",
    code: "OU-001",
    name: "Hargeisa North",
    city: "Hargeisa",
    countryCode: "SO",
    latitude: "9.5700",
    longitude: "44.0600",
    status: "Active",
  },
  {
    id: 2,
    tenantId: "TEN-003",
    code: "OU-007",
    name: "Nairobi Eastleigh",
    city: "Nairobi",
    countryCode: "KE",
    latitude: "-1.2720",
    longitude: "36.8500",
    status: "Active",
  },
  {
    id: 3,
    tenantId: "TEN-004",
    code: "OU-010",
    name: "Dubai Deira",
    city: "Dubai",
    countryCode: "AE",
    latitude: "25.2750",
    longitude: "55.3070",
    status: "Active",
  },
  {
    id: 4,
    tenantId: "TEN-005",
    code: "OU-013",
    name: "London Wembley",
    city: "London",
    countryCode: "GB",
    latitude: "51.5520",
    longitude: "-0.2960",
    status: "Active",
  },
];

const emptyForm: OrgUnitFormState = {
  tenantId: "",
  code: "",
  name: "",
  city: "",
  countryCode: "",
  latitude: "",
  longitude: "",
  status: "Active",
};

function resolveTenantName(tenantId: string) {
  return tenantOptions.find((t) => t.id === tenantId)?.name ?? tenantId;
}

function corridorLabel(countryCode: string) {
  switch (countryCode.toUpperCase()) {
    case "SO":
      return "Somalia/Land corridor";
    case "AE":
      return "UAE corridor";
    case "GB":
      return "UK corridor";
    case "KE":
      return "Kenya corridor";
    default:
      return "Other corridor";
  }
}

export default function OrgUnitsPage() {
  const [rows, setRows] = useState<OrgUnitRecord[]>(initialRows);
  const [query, setQuery] = useState("");
  const [tenantFilter, setTenantFilter] = useState("all");
  const [statusFilter, setStatusFilter] = useState("all");

  const [form, setForm] = useState<OrgUnitFormState>(emptyForm);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [message, setMessage] = useState<string>("");

  useEffect(() => {
    if (!message) return;
    const timer = window.setTimeout(() => setMessage(""), 3000);
    return () => window.clearTimeout(timer);
  }, [message]);

  const filteredRows = useMemo(() => {
    const q = query.trim().toLowerCase();

    return rows.filter((row) => {
      const matchesQuery =
        !q ||
        row.code.toLowerCase().includes(q) ||
        row.name.toLowerCase().includes(q) ||
        row.city.toLowerCase().includes(q) ||
        row.countryCode.toLowerCase().includes(q) ||
        resolveTenantName(row.tenantId).toLowerCase().includes(q);

      const matchesTenant = tenantFilter === "all" || row.tenantId === tenantFilter;
      const matchesStatus = statusFilter === "all" || row.status === statusFilter;

      return matchesQuery && matchesTenant && matchesStatus;
    });
  }, [rows, query, tenantFilter, statusFilter]);

  const activeCount = useMemo(
    () => rows.filter((row) => row.status === "Active").length,
    [rows]
  );

  const byCorridor = useMemo(() => {
    const counts = rows.reduce<Record<string, number>>((acc, row) => {
      const key = corridorLabel(row.countryCode);
      acc[key] = (acc[key] ?? 0) + 1;
      return acc;
    }, {});
    return counts;
  }, [rows]);

  const topCorridors = useMemo(() => {
    const entries = Object.entries(byCorridor).sort((a, b) => b[1] - a[1]);
    return {
      first: entries[0] ?? ["Somalia/Land corridor", 0],
      second: entries[1] ?? ["UAE corridor", 0],
      third: entries[2] ?? ["UK corridor", 0],
    };
  }, [byCorridor]);

  function openCreateForm() {
    setEditingId(null);
    setForm(emptyForm);
    setIsFormOpen(true);
  }

  function openEditForm(row: OrgUnitRecord) {
    setEditingId(row.id);
    setForm({
      tenantId: row.tenantId,
      code: row.code,
      name: row.name,
      city: row.city,
      countryCode: row.countryCode,
      latitude: row.latitude,
      longitude: row.longitude,
      status: row.status,
    });
    setIsFormOpen(true);
  }

  function closeForm() {
    setEditingId(null);
    setForm(emptyForm);
    setIsFormOpen(false);
  }

  function updateForm<K extends keyof OrgUnitFormState>(key: K, value: OrgUnitFormState[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault();

    if (!form.tenantId || !form.code || !form.name || !form.city || !form.countryCode) {
      setMessage("Please complete tenant, code, name, city, and country.");
      return;
    }

    if (editingId !== null) {
      setRows((prev) =>
        prev.map((row) =>
          row.id === editingId
            ? {
                ...row,
                tenantId: form.tenantId,
                code: form.code.trim().toUpperCase(),
                name: form.name.trim(),
                city: form.city.trim(),
                countryCode: form.countryCode.trim().toUpperCase(),
                latitude: form.latitude.trim(),
                longitude: form.longitude.trim(),
                status: form.status,
              }
            : row
        )
      );
      setMessage("Org unit updated successfully.");
    } else {
      const nextId = rows.length ? Math.max(...rows.map((r) => r.id)) + 1 : 1;
      setRows((prev) => [
        {
          id: nextId,
          tenantId: form.tenantId,
          code: form.code.trim().toUpperCase(),
          name: form.name.trim(),
          city: form.city.trim(),
          countryCode: form.countryCode.trim().toUpperCase(),
          latitude: form.latitude.trim(),
          longitude: form.longitude.trim(),
          status: form.status,
        },
        ...prev,
      ]);
      setMessage("Org unit created successfully.");
    }

    closeForm();
  }

  function toggleStatus(id: number) {
    setRows((prev) =>
      prev.map((row) =>
        row.id === id
          ? {
              ...row,
              status: row.status === "Active" ? "Inactive" : "Active",
            }
          : row
      )
    );
    setMessage("Org unit status updated.");
  }

  return (
    <div>
      <section className="hero">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <p className="hero-eyebrow">Platform Control Center</p>
            <h1 className="hero-title">Org Units</h1>
            <p className="hero-copy">
              Manage branches, corridors, and operational locations across tenants.
            </p>
          </div>

          <div className="panel-header__action">
            <AppButton variant="primary" onClick={openCreateForm}>
              Add Org Unit
            </AppButton>
          </div>
        </div>
      </section>

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Active branches</div>
          <div className="kpi-value">{activeCount}</div>
          <div className="kpi-meta">Across {new Set(rows.map((r) => r.tenantId)).size} tenants</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">{topCorridors.first[0]}</div>
          <div className="kpi-value">{topCorridors.first[1]}</div>
          <div className="kpi-meta">Largest network</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">{topCorridors.second[0]}</div>
          <div className="kpi-value">{topCorridors.second[1]}</div>
          <div className="kpi-meta">Operational corridor</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">{topCorridors.third[0]}</div>
          <div className="kpi-value">{topCorridors.third[1]}</div>
          <div className="kpi-meta">Operational corridor</div>
        </div>
      </section>

      <section className="panel-block" style={{ marginBottom: 20 }}>
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <h2>Org unit management</h2>
            <p className="panel-subtitle">
              Create new org units, update branch details, and manage active status.
            </p>
          </div>
        </div>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "2fr 1fr 1fr auto",
            gap: 12,
            alignItems: "end",
          }}
        >
          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Search</div>
            <input
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search code, name, tenant, city, country"
              style={inputStyle}
            />
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Tenant</div>
            <select
              value={tenantFilter}
              onChange={(e) => setTenantFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All tenants</option>
              {tenantOptions.map((tenant) => (
                <option key={tenant.id} value={tenant.id}>
                  {tenant.name}
                </option>
              ))}
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Status</div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All statuses</option>
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
          </label>

          <AppButton
            variant="secondary"
            onClick={() => {
              setQuery("");
              setTenantFilter("all");
              setStatusFilter("all");
            }}
          >
            Reset
          </AppButton>
        </div>

        {message ? (
          <div
            style={{
              marginTop: 14,
              padding: "12px 14px",
              borderRadius: 12,
              background: "#edf4e8",
              color: "#2f6f2c",
              fontWeight: 700,
            }}
          >
            {message}
          </div>
        ) : null}
      </section>

      {isFormOpen ? (
        <section className="panel-block" style={{ marginBottom: 20 }}>
          <div className="panel-header">
            <h2>{editingId !== null ? "Edit org unit" : "Add org unit"}</h2>
            <AppButton variant="secondary" onClick={closeForm}>
              Cancel
            </AppButton>
          </div>

          <form onSubmit={handleSubmit}>
            <div
              style={{
                display: "grid",
                gridTemplateColumns: "repeat(4, minmax(0, 1fr))",
                gap: 14,
              }}
            >
              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Tenant</div>
                <select
                  value={form.tenantId}
                  onChange={(e) => updateForm("tenantId", e.target.value)}
                  style={inputStyle}
                >
                  <option value="">Select tenant</option>
                  {tenantOptions.map((tenant) => (
                    <option key={tenant.id} value={tenant.id}>
                      {tenant.name}
                    </option>
                  ))}
                </select>
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Code</div>
                <input
                  value={form.code}
                  onChange={(e) => updateForm("code", e.target.value)}
                  placeholder="OU-016"
                  style={inputStyle}
                />
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Name</div>
                <input
                  value={form.name}
                  onChange={(e) => updateForm("name", e.target.value)}
                  placeholder="Sharjah Central"
                  style={inputStyle}
                />
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Status</div>
                <select
                  value={form.status}
                  onChange={(e) => updateForm("status", e.target.value as "Active" | "Inactive")}
                  style={inputStyle}
                >
                  <option value="Active">Active</option>
                  <option value="Inactive">Inactive</option>
                </select>
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>City</div>
                <input
                  value={form.city}
                  onChange={(e) => updateForm("city", e.target.value)}
                  placeholder="Dubai"
                  style={inputStyle}
                />
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Country</div>
                <input
                  value={form.countryCode}
                  onChange={(e) => updateForm("countryCode", e.target.value)}
                  placeholder="AE"
                  style={inputStyle}
                />
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Latitude</div>
                <input
                  value={form.latitude}
                  onChange={(e) => updateForm("latitude", e.target.value)}
                  placeholder="25.2048"
                  style={inputStyle}
                />
              </label>

              <label>
                <div className="cell-muted" style={{ marginBottom: 6 }}>Longitude</div>
                <input
                  value={form.longitude}
                  onChange={(e) => updateForm("longitude", e.target.value)}
                  placeholder="55.2708"
                  style={inputStyle}
                />
              </label>
            </div>

            <div style={{ display: "flex", gap: 12, marginTop: 18 }}>
              <AppButton variant="primary" type="submit">
                {editingId !== null ? "Save Changes" : "Create Org Unit"}
              </AppButton>

              <AppButton variant="secondary" type="button" onClick={closeForm}>
                Cancel
              </AppButton>
            </div>
          </form>
        </section>
      ) : null}

      <section className="panel-block">
        <div className="panel-header">
          <h2>Org units</h2>
          <div className="cell-muted">{filteredRows.length} records</div>
        </div>

        <div className="table-shell table-wide">
          <table className="data-table">
            <thead>
              <tr>
                <th className="cell-mono">Code</th>
                <th>Org Unit</th>
                <th>Tenant</th>
                <th>City</th>
                <th>Country</th>
                <th>Status</th>
                <th className="cell-wrap">Coordinates</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredRows.length === 0 ? (
                <tr>
                  <td colSpan={8} className="cell-muted">
                    No org units match your current filters.
                  </td>
                </tr>
              ) : (
                filteredRows.map((row) => (
                  <tr key={row.id}>
                    <td className="cell-mono">{row.code}</td>
                    <td>{row.name}</td>
                    <td>{resolveTenantName(row.tenantId)}</td>
                    <td>{row.city}</td>
                    <td>{row.countryCode}</td>
                    <td>
                      <span
                        className={
                          row.status === "Active"
                            ? "status-badge status-badge--success"
                            : "status-badge status-badge--default"
                        }
                      >
                        {row.status}
                      </span>
                    </td>
                    <td className="cell-wrap cell-mono">
                      {row.latitude || "-"}, {row.longitude || "-"}
                    </td>
                    <td>
                      <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                        <AppButton variant="secondary" onClick={() => openEditForm(row)}>
                          Edit
                        </AppButton>
                        <AppButton
                          variant={row.status === "Active" ? "warning" : "primary"}
                          onClick={() => toggleStatus(row.id)}
                        >
                          {row.status === "Active" ? "Deactivate" : "Activate"}
                        </AppButton>
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>
    </div>
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