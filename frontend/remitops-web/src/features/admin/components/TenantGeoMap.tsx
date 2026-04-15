import { MapContainer, Marker, Popup, TileLayer } from "react-leaflet";
import type { MapContainerProps } from "react-leaflet";
import type { LatLngExpression } from "leaflet";
import "leaflet/dist/leaflet.css";
import type { TenantDto } from "../api/tenantsApi";

type Props = {
  tenants: TenantDto[];
};

export default function TenantGeoMap({ tenants }: Props) {
  const points = tenants.filter((t) => {
    const lat = t.latitude as number | null | undefined;
    const lng = t.longitude as number | null | undefined;
    return typeof lat === "number" && !Number.isNaN(lat) &&
           typeof lng === "number" && !Number.isNaN(lng);
  });

  const hasPoints = points.length > 0;

  const center: LatLngExpression = hasPoints
    ? [points[0].latitude as number, points[0].longitude as number]
    : [9.5, 44.0];

  const mapProps: MapContainerProps = {
    center,
    zoom: hasPoints ? 4 : 3,
    style: { height: "100%", width: "100%" },
  };

  return (
    <div
      style={{
        height: 360,
        width: "100%",
        overflow: "hidden",
        borderRadius: 18,
      }}
    >
      <MapContainer {...mapProps}>
        <TileLayer
          attribution='&copy; OpenStreetMap contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        {points.map((tenant) => (
          <Marker
            key={tenant.id}
            position={[
              tenant.latitude as number,
              tenant.longitude as number,
            ]}
          >
            <Popup>
              <strong>{tenant.name}</strong>
              <br />
              {tenant.city ?? "-"}, {tenant.countryCode ?? "-"}
            </Popup>
          </Marker>
        ))}
      </MapContainer>
    </div>
  );
}