import {
  flexRender,
  getCoreRowModel,
  useReactTable,
} from '@tanstack/react-table'
import type { ColumnDef } from '@tanstack/react-table'

type Txn = {
  id: string
  sender: string
  corridor: string
  amount: number
  status: 'Completed' | 'Pending' | 'Failed'
  time: string
}

export default function TransactionsTable({ data }: { data: Txn[] }) {
  const columns: ColumnDef<Txn>[] = [
    { accessorKey: 'id', header: 'Txn ID' },
    { accessorKey: 'sender', header: 'Sender' },
    { accessorKey: 'corridor', header: 'Corridor' },
    { accessorKey: 'amount', header: 'Amount' },
    {
      accessorKey: 'status',
      header: 'Status',
      cell: ({ getValue }) => {
        const status = getValue<string>()
        const tone =
          status === 'Completed'
            ? 'badge-success'
            : status === 'Pending'
            ? 'badge-warning'
            : 'badge-danger'

        return <span className={`badge ${tone}`}>{status}</span>
      },
    },
    { accessorKey: 'time', header: 'Time' },
  ]

  const table = useReactTable({
    data,
    columns,
    getCoreRowModel: getCoreRowModel(),
  })

  return (
    <div className="panel">
      <div className="panel-title">
        <h3>Recent Transactions</h3>
        <span className="panel-subtitle">Latest operational events</span>
      </div>

      <div className="table-wrap">
        <table className="txn-table">
          <thead>
            {table.getHeaderGroups().map((headerGroup) => (
              <tr key={headerGroup.id}>
                {headerGroup.headers.map((header) => (
                  <th key={header.id}>
                    {header.isPlaceholder
                      ? null
                      : flexRender(header.column.columnDef.header, header.getContext())}
                  </th>
                ))}
              </tr>
            ))}
          </thead>

          <tbody>
            {table.getRowModel().rows.map((row) => (
              <tr key={row.id}>
                {row.getVisibleCells().map((cell) => (
                  <td key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </td>
                ))}
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  )
}