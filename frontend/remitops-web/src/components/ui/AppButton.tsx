import React from "react";

type AppButtonVariant = "primary" | "secondary" | "warning";

type AppButtonProps = {
  children: React.ReactNode;
  variant?: AppButtonVariant;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  className?: string;
  onClick?: () => void;
};

export default function AppButton({
  children,
  variant = "primary",
  type = "button",
  disabled = false,
  className = "",
  onClick,
}: AppButtonProps) {
  return (
    <button
      type={type}
      disabled={disabled}
      onClick={onClick}
      className={`btn btn-${variant} ${className}`.trim()}
    >
      {children}
    </button>
  );
}