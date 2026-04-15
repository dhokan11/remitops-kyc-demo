import React from "react";

type PageHeaderProps = {
  title: string;
  subtitle?: string;
  action?: React.ReactNode;
  eyebrow?: string;
};

export default function PageHeader({
  title,
  subtitle,
  action,
  eyebrow = "Platform control center",
}: PageHeaderProps) {
  return (
    <section className="page-hero card">
      <div>
        <div className="page-eyebrow">{eyebrow}</div>
        <h1>{title}</h1>
        {subtitle ? <p>{subtitle}</p> : null}
      </div>

      {action ? <div className="hero-actions">{action}</div> : null}
    </section>
  );
}