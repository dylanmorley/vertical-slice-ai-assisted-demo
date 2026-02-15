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
import { getConfig } from './config'
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

  const config = getConfig()

  // Create a stable redirect callback function
  const redirectCallback = useCallback((appState) => {
    if (appState?.returnTo) {
      window.location.href = appState.returnTo
    }
  }, [])

  const providerConfig = {
    domain: config.domain,
    clientId: config.clientId,
    useRedirectCallback: redirectCallback,
    authorizationParams: {
      redirect_uri: window.location.origin,
      scope: 'openid profile email',
      ...(config.audience ? { audience: config.audience } : null),
    },
  }

  return (
    <Auth0Provider {...providerConfig}>
      <App />
    </Auth0Provider>
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
