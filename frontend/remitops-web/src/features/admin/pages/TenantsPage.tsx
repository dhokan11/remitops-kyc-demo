import { useEffect, useMemo, useState } from "react";
import type { CSSProperties, SyntheticEvent } from "react";
import AppButton from "../../../components/ui/AppButton";
import { tenantsApi } from "../api/tenantsApi";
import type { TenantDto, UpsertTenantRequest } from "../api/tenantsApi";

type TenantFormState = {
  code: string;
  name: string;
  city: string;
  countryCode: string;
  latitude: string;
  longitude: string;
  status: string;
};

const emptyForm: TenantFormState = {
  code: "",
  name: "",
  city: "",
  countryCode: "",
  latitude: "",
  longitude: "",
  status: "Active",
};

function regionLabel(countryCode: string | null) {
  if (!countryCode) return "Unmapped";

  switch (countryCode.toUpperCase()) {
    case "SO":
    case "SOMALIA":
      return "Somalia/Land corridor";
    case "KE":
    case "KENYA":
      return "East Africa hub";
    case "AE":
      return "Gulf hub";
    case "GB":
    case "UK":
      return "UK / Europe";
    default:
      return "Other region";
  }
}

function getTenantStatus(tenant: TenantDto) {
  return tenant.isActive ? "Active" : "Inactive";
}

function mapToForm(tenant: TenantDto): TenantFormState {
  return {
    code: tenant.code ?? "",
    name: tenant.name ?? "",
    city: tenant.city ?? "",
    countryCode: tenant.countryCode ?? "",
    latitude: tenant.latitude?.toString() ?? "",
    longitude: tenant.longitude?.toString() ?? "",
    status: tenant.isActive ? "Active" : "Inactive",
  };
}

function mapToPayload(form: TenantFormState): UpsertTenantRequest {
  const toNumber = (value: string) =>
    value.trim() === "" ? null : Number(value.trim());

  return {
    code: form.code.trim().toUpperCase(),
    name: form.name.trim(),
    countryCode: form.countryCode.trim() || null,
    city: form.city.trim() || null,
    latitude: toNumber(form.latitude),
    longitude: toNumber(form.longitude),
    isActive: form.status === "Active",
  };
}

export default function TenantsPage() {
  const [rows, setRows] = useState<TenantDto[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const [query, setQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<"all" | "Active" | "Inactive">("all");
  const [regionFilter, setRegionFilter] = useState("all");

  const [form, setForm] = useState<TenantFormState>(emptyForm);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | null>(null);
  const [toast, setToast] = useState("");

  async function loadTenants() {
    setIsLoading(true);
    setError(null);

    try {
      const data = await tenantsApi.list();
      setRows(data);
    } catch (err: any) {
      setError(err.message || "Failed to load tenants.");
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    loadTenants();
  }, []);

  useEffect(() => {
    if (!toast) return;
    const id = window.setTimeout(() => setToast(""), 3000);
    return () => window.clearTimeout(id);
  }, [toast]);

  const filteredRows = useMemo(() => {
    const q = query.trim().toLowerCase();

    return rows.filter((row) => {
      const matchesQuery =
        !q ||
        (row.code ?? "").toLowerCase().includes(q) ||
        (row.name ?? "").toLowerCase().includes(q) ||
        (row.city ?? "").toLowerCase().includes(q) ||
        (row.countryCode ?? "").toLowerCase().includes(q);

      const status = getTenantStatus(row);
      const matchesStatus = statusFilter === "all" || status === statusFilter;

      const matchesRegion =
        regionFilter === "all" || regionLabel(row.countryCode) === regionFilter;

      return matchesQuery && matchesStatus && matchesRegion;
    });
  }, [rows, query, statusFilter, regionFilter]);

  const activeCount = useMemo(() => rows.filter((t) => t.isActive).length, [rows]);

  const byRegion = useMemo(() => {
    const counts = rows.reduce<Record<string, number>>((acc, row) => {
      const key = regionLabel(row.countryCode);
      acc[key] = (acc[key] ?? 0) + 1;
      return acc;
    }, {});
    return counts;
  }, [rows]);

  const topRegions = useMemo(() => {
    const entries = Object.entries(byRegion).sort((a, b) => b[1] - a[1]);
    return {
      first: entries[0] ?? ["Key corridor", 0],
      second: entries[1] ?? ["Secondary corridor", 0],
      third: entries[2] ?? ["Emerging corridor", 0],
    };
  }, [byRegion]);

  function openCreateModal() {
    setEditingId(null);
    setForm(emptyForm);
    setIsModalOpen(true);
  }

  function openEditModal(tenant: TenantDto) {
    setEditingId(tenant.id);
    setForm(mapToForm(tenant));
    setIsModalOpen(true);
  }

  function closeModal() {
    setEditingId(null);
    setForm(emptyForm);
    setIsModalOpen(false);
  }

  function updateForm<K extends keyof TenantFormState>(key: K, value: TenantFormState[K]) {
    setForm((prev) => ({ ...prev, [key]: value }));
  }

  async function handleSubmit(event: SyntheticEvent<HTMLFormElement>) {
    event.preventDefault();

    if (!form.code.trim() || !form.name.trim()) {
      setToast("Code and name are required.");
      return;
    }

    const payload = mapToPayload(form);

    try {
      if (editingId) {
        await tenantsApi.update(editingId, payload);
        await loadTenants();
        setToast("Tenant updated.");
      } else {
        const created = await tenantsApi.create(payload);
        setRows((prev) => [created, ...prev]);
        setToast("Tenant created.");
      }

      closeModal();
    } catch (err: any) {
      setToast(err.message || "Failed to save tenant.");
    }
  }

  async function toggleStatus(id: string, currentIsActive: boolean) {
    try {
      const updated = await tenantsApi.toggleStatus(id, !currentIsActive);
      setRows((prev) => prev.map((r) => (r.id === updated.id ? updated : r)));
      setToast("Tenant status updated.");
    } catch (err: any) {
      setToast(err.message || "Failed to update status.");
    }
  }

  return (
    <div>
      <section className="hero">
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <p className="hero-eyebrow">Platform Control Center</p>
            <h1 className="hero-title">Tenants</h1>
            <p className="hero-copy">
              Create and manage tenant entities across the platform.
            </p>
          </div>

          <div className="panel-header__action">
            <AppButton variant="primary" onClick={openCreateModal}>
              Add Tenant
            </AppButton>
          </div>
        </div>
      </section>

      <section className="kpi-grid">
        <div className="card kpi-card">
          <div className="kpi-label">Active tenants</div>
          <div className="kpi-value">{activeCount}</div>
          <div className="kpi-meta">From {rows.length} total</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">{topRegions.first[0]}</div>
          <div className="kpi-value">{topRegions.first[1]}</div>
          <div className="kpi-meta">Largest distribution</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">{topRegions.second[0]}</div>
          <div className="kpi-value">{topRegions.second[1]}</div>
          <div className="kpi-meta">Secondary coverage</div>
        </div>

        <div className="card kpi-card">
          <div className="kpi-label">{topRegions.third[0]}</div>
          <div className="kpi-value">{topRegions.third[1]}</div>
          <div className="kpi-meta">Emerging corridor</div>
        </div>
      </section>

      <section className="panel-block" style={{ marginBottom: 20 }}>
        <div className="panel-header panel-header--stackable">
          <div className="panel-header__copy">
            <h2>Tenant management</h2>
            <p className="panel-subtitle">
              Filter, search, and maintain tenant details and operational status.
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
              placeholder="Search code, name, city, country"
              style={inputStyle}
            />
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Status</div>
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value as "all" | "Active" | "Inactive")}
              style={inputStyle}
            >
              <option value="all">All statuses</option>
              <option value="Active">Active</option>
              <option value="Inactive">Inactive</option>
            </select>
          </label>

          <label>
            <div className="cell-muted" style={{ marginBottom: 6 }}>Region</div>
            <select
              value={regionFilter}
              onChange={(e) => setRegionFilter(e.target.value)}
              style={inputStyle}
            >
              <option value="all">All regions</option>
              <option value="Somalia/Land corridor">Somalia/Land corridor</option>
              <option value="East Africa hub">East Africa hub</option>
              <option value="Gulf hub">Gulf hub</option>
              <option value="UK / Europe">UK / Europe</option>
              <option value="Unmapped">Unmapped</option>
              <option value="Other region">Other region</option>
            </select>
          </label>

          <AppButton
            variant="secondary"
            onClick={() => {
              setQuery("");
              setStatusFilter("all");
              setRegionFilter("all");
            }}
          >
            Reset
          </AppButton>
        </div>

        {isLoading && (
          <div className="loading-state" style={{ marginTop: 14 }}>
            Loading tenants…
          </div>
        )}

        {error && !isLoading && (
          <div className="alert alert-danger" style={{ marginTop: 14 }}>
            {error}
          </div>
        )}

        {toast && (
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
            {toast}
          </div>
        )}
      </section>

      <section className="panel-block">
        <div className="panel-header">
          <h2>Tenants</h2>
          <div className="cell-muted">{filteredRows.length} records</div>
        </div>

        <div className="table-shell table-wide">
          <table className="data-table">
            <thead>
              <tr>
                <th className="cell-mono">Code</th>
                <th>Name</th>
                <th>City</th>
                {/* <th>Country</th> */}
                {/* <th>Region</th> */}
                <th>Status</th>
                <th className="cell-wrap">Coordinates</th>
                {/* <th>Created (UTC)</th> */}
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              {!isLoading && filteredRows.length === 0 ? (
                <tr>
                  <td colSpan={6} className="cell-muted">
                    No tenants match your current filters.
                  </td>
                </tr>
              ) : (
                filteredRows.map((row) => {
                  const status = getTenantStatus(row);

                  return (
                    <tr key={row.id}>
                      <td className="cell-mono">{row.code}</td>
                      <td>{row.name}</td>
                      <td>{row.city ?? "-"}</td>
                      {/* <td>{row.countryCode ?? "-"}</td> */}
                      {/* <td className="cell-wrap">{regionLabel(row.countryCode)}</td> */}
                      <td>
                        <span
                          className={
                            row.isActive
                              ? "status-badge status-badge--success"
                              : "status-badge status-badge--default"
                          }
                        >
                          {status}
                        </span>
                      </td>
                      <td className="cell-wrap cell-mono">
                        {row.latitude ?? "-"}, {row.longitude ?? "-"}
                      </td>
                      {/* <td className="cell-mono">
                        {row.createdAtUtc?.replace("T", " ").replace("Z", "")}
                      </td> */}
                      <td>
                        <div style={{ display: "flex", gap: 8, flexWrap: "wrap" }}>
                          <AppButton variant="secondary" onClick={() => openEditModal(row)}>
                            Edit
                          </AppButton>
                          <AppButton
                            variant={row.isActive ? "warning" : "primary"}
                            onClick={() => toggleStatus(row.id, row.isActive)}
                          >
                            {row.isActive ? "Deactivate" : "Activate"}
                          </AppButton>
                        </div>
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </section>

      {isModalOpen && (
        <>
          <button
            type="button"
            className="sidebar-backdrop"
            onClick={closeModal}
            aria-label="Close tenant modal"
          />
          <div
            style={{
              position: "fixed",
              inset: 0,
              display: "grid",
              placeItems: "center",
              zIndex: 40,
              pointerEvents: "none",
            }}
          >
            <div
              className="panel-block"
              style={{
                width: "min(640px, 100% - 32px)",
                maxHeight: "90vh",
                overflow: "auto",
                pointerEvents: "auto",
              }}
            >
              <div className="panel-header">
                <h2>{editingId ? "Edit tenant" : "Add tenant"}</h2>
                <AppButton variant="secondary" type="button" onClick={closeModal}>
                  Close
                </AppButton>
              </div>

              <form onSubmit={handleSubmit}>
                <div
                  style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(2, minmax(0, 1fr))",
                    gap: 14,
                  }}
                >
                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>Code</div>
                    <input
                      value={form.code}
                      onChange={(e) => updateForm("code", e.target.value)}
                      placeholder="TEN-006"
                      style={inputStyle}
                    />
                  </label>

                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>Name</div>
                    <input
                      value={form.name}
                      onChange={(e) => updateForm("name", e.target.value)}
                      placeholder="Dahab Kigali"
                      style={inputStyle}
                    />
                  </label>

                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>City</div>
                    <input
                      value={form.city}
                      onChange={(e) => updateForm("city", e.target.value)}
                      placeholder="Kigali"
                      style={inputStyle}
                    />
                  </label>

                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>Country</div>
                    <input
                      value={form.countryCode}
                      onChange={(e) => updateForm("countryCode", e.target.value)}
                      placeholder="SO"
                      style={inputStyle}
                    />
                  </label>

                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>Latitude</div>
                    <input
                      value={form.latitude}
                      onChange={(e) => updateForm("latitude", e.target.value)}
                      placeholder="8.41"
                      style={inputStyle}
                    />
                  </label>

                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>Longitude</div>
                    <input
                      value={form.longitude}
                      onChange={(e) => updateForm("longitude", e.target.value)}
                      placeholder="48.48"
                      style={inputStyle}
                    />
                  </label>

                  <label>
                    <div className="cell-muted" style={{ marginBottom: 6 }}>Status</div>
                    <select
                      value={form.status}
                      onChange={(e) => updateForm("status", e.target.value)}
                      style={inputStyle}
                    >
                      <option value="Active">Active</option>
                      <option value="Inactive">Inactive</option>
                    </select>
                  </label>
                </div>

                <div style={{ display: "flex", gap: 12, marginTop: 18 }}>
                  <AppButton variant="primary" type="submit">
                    {editingId ? "Save Changes" : "Create Tenant"}
                  </AppButton>
                  <AppButton variant="secondary" type="button" onClick={closeModal}>
                    Cancel
                  </AppButton>
                </div>
              </form>
            </div>
          </div>
        </>
      )}
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