import React from 'react'
import { useUser } from '../../contexts/UserContext'
import './Dashboard.scss'

const Dashboard = () => {
  const { fullName } = useUser()

  const styles = {
    container: {
      padding: '1.5rem',
      maxWidth: '1600px',
      margin: '0 auto',
    },
    greeting: {
      fontSize: '1.5rem',
      fontWeight: 600,
      color: 'var(--cui-body-color)',
    },
  }

  return (
    <div style={styles.container} className="dashboard">
      <div style={styles.greeting}>Welcome back, {fullName || 'User'}</div>
    </div>
  )
}

export default Dashboard
