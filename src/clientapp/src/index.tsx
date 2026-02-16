import React, { useEffect, useCallback } from 'react'
import { createRoot } from 'react-dom/client'
import { Provider } from 'react-redux'
import App from './App'
import Cookies from 'js-cookie'
import store from './store'
import * as serviceWorker from './serviceWorker'
import { Auth0Provider } from '@auth0/auth0-react'
import './index.css'
import 'core-js'
import ErrorBoundary from './components/ErrorBoundary'

function AppWrapper() {
  const searchParams = new URLSearchParams(window.location.search)
  const page = searchParams.get('page')

  useEffect(() => {
    if (page === 'app') {
      Cookies.set('pageMode', 'app')
    } else if (page === 'holding') {
      Cookies.set('pageMode', 'holding')
    }
  }, [page])

  return (
      <App />
  )
}

createRoot(document.getElementById('root')).render(
  <ErrorBoundary>
    <Provider store={store}>
      <AppWrapper />
    </Provider>
  </ErrorBoundary>,
)

serviceWorker.unregister()
