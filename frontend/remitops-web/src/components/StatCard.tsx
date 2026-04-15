import { useEffect, useRef } from 'react'
import { CountUp } from 'countup.js'
import type { LucideIcon } from 'lucide-react'

type StatCardProps = {
  title: string
  value: number
  subtitle: string
  icon: LucideIcon
  tone?: 'default' | 'success' | 'danger' | 'accent'
  prefix?: string
}

export default function StatCard({
  title,
  value,
  subtitle,
  icon: Icon,
  tone = 'default',
  prefix = '',
}: StatCardProps) {
  const numberRef = useRef<HTMLHeadingElement>(null)

  useEffect(() => {
    if (!numberRef.current) return
    const counter = new CountUp(numberRef.current, value, {
      duration: 1.8,
      prefix,
      useGrouping: true,
    })
    if (!counter.error) counter.start()
  }, [value, prefix])

  return (
    <div className={`card ${tone}`}>
      <div className="card-top">
        <span>{title}</span>
        <Icon size={20} />
      </div>
      <h2 ref={numberRef}>0</h2>
      <small>{subtitle}</small>
    </div>
  )
}