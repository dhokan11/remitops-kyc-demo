type StatusTone = "success" | "warning" | "danger" | "neutral";

type StatusBadgeProps = {
  children: React.ReactNode;
  tone?: StatusTone;
};

export default function StatusBadge({
  children,
  tone = "neutral",
}: StatusBadgeProps) {
  return <span className={`badge badge-${tone}`}>{children}</span>;
}

export function getStatusTone(value?: string): StatusTone {
  const normalized = String(value || "").toLowerCase();

  if (
    ["active", "approved", "completed", "success", "healthy", "enabled"].includes(
      normalized
    )
  ) {
    return "success";
  }

  if (
    ["pending", "queued", "review", "warning", "in progress"].includes(normalized)
  ) {
    return "warning";
  }

  if (
    ["inactive", "flagged", "failed", "rejected", "disabled", "error"].includes(
      normalized
    )
  ) {
    return "danger";
  }

  return "neutral";
}