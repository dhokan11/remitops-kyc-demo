import React from "react";

type Column<T> = {
  key: keyof T | string;
  header: string;
  className?: string;
  render?: (value: any, row: T) => React.ReactNode;
};

type DataTableProps<T> = {
  rows: T[];
  columns: Column<T>[];
  rowKey?: (row: T, index: number) => string;
};

export default function DataTable<T extends Record<string, any>>({
  rows,
  columns,
  rowKey,
}: DataTableProps<T>) {
  return (
    <div className="table-wrap">
      <table className="data-table">
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={String(column.key)} className={column.className}>
                {column.header}
              </th>
            ))}
          </tr>
        </thead>

        <tbody>
          {rows.map((row, index) => (
            <tr key={rowKey ? rowKey(row, index) : String(row.id ?? index)}>
              {columns.map((column) => {
                const value = row[column.key as keyof T];
                return (
                  <td key={String(column.key)} className={column.className}>
                    {column.render ? column.render(value, row) : String(value ?? "")}
                  </td>
                );
              })}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}