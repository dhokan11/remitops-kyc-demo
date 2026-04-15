export const trendData = [
  { day: 'Mon', tx: 18 },
  { day: 'Tue', tx: 24 },
  { day: 'Wed', tx: 21 },
  { day: 'Thu', tx: 28 },
  { day: 'Fri', tx: 30 },
  { day: 'Sat', tx: 26 },
  { day: 'Sun', tx: 34 },
]

export const recentTransactions = [
  {
    id: 'TXN-1042',
    sender: 'Amina Yusuf',
    corridor: 'UK → Somalia',
    amount: 420,
    status: 'Completed' as const,
    time: '2 min ago',
  },
  {
    id: 'TXN-1041',
    sender: 'Omar Ali',
    corridor: 'UAE → Kenya',
    amount: 275,
    status: 'Pending' as const,
    time: '8 min ago',
  },
  {
    id: 'TXN-1040',
    sender: 'Hodan Noor',
    corridor: 'US → Ethiopia',
    amount: 600,
    status: 'Failed' as const,
    time: '14 min ago',
  },
  {
    id: 'TXN-1039',
    sender: 'Abdi Hassan',
    corridor: 'Canada → Somalia',
    amount: 180,
    status: 'Completed' as const,
    time: '22 min ago',
  },
]