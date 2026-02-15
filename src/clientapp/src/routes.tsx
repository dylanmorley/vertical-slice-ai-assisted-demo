import React from 'react'

const Dashboard = React.lazy(() => import('./views/dashboard/Dashboard'))
const AuditAdmin = React.lazy(() => import('./views/administration/AuditAdmin'))

const routes = [
  { path: '/', exact: true, name: 'Home' },
  { path: '/dashboard', name: 'Dashboard', element: Dashboard },
  { path: '/administration/audit', name: 'Audit Records', element: AuditAdmin },
]

export default routes
