import React from 'react'
import { useLocation, Link } from 'react-router-dom'

import routes from '../routes'

import { CBreadcrumb, CBreadcrumbItem } from '@coreui/react'

interface RouteConfig {
  path: string
  name: string
  element?: React.ComponentType
  exact?: boolean
}

interface BreadcrumbItem {
  pathname: string
  name: string
  active: boolean
}

const AppBreadcrumb = () => {
  const currentLocation = useLocation().pathname

  const getRouteName = (pathname: string, routeList: RouteConfig[]): string | false => {
    const currentRoute = routeList.find((route) => route.path === pathname)
    return currentRoute ? currentRoute.name : false
  }

  const getBreadcrumbs = (location: string): BreadcrumbItem[] => {
    const breadcrumbs: BreadcrumbItem[] = []
    location.split('/').reduce((prev, curr, index, array) => {
      const currentPathname = `${prev}/${curr}`
      const routeName = getRouteName(currentPathname, routes as RouteConfig[])
      if (routeName) {
        breadcrumbs.push({
          pathname: currentPathname,
          name: routeName,
          active: index + 1 === array.length,
        })
      }
      return currentPathname
    }, '')
    return breadcrumbs
  }

  const breadcrumbs = getBreadcrumbs(currentLocation)

  return (
    <CBreadcrumb className="my-0">
      <CBreadcrumbItem>
        <Link to="/" style={{ textDecoration: 'none', color: 'inherit' }}>
          Home
        </Link>
      </CBreadcrumbItem>
      {breadcrumbs.map((breadcrumb, index) => {
        // Special case: Nodes Management should link to /nodes
        const linkPath = breadcrumb.name === 'Nodes Management' ? '/nodes' : breadcrumb.pathname

        return (
          <CBreadcrumbItem active={breadcrumb.active} key={index}>
            {breadcrumb.active ? (
              breadcrumb.name
            ) : (
              <Link to={linkPath} style={{ textDecoration: 'none', color: 'inherit' }}>
                {breadcrumb.name}
              </Link>
            )}
          </CBreadcrumbItem>
        )
      })}
    </CBreadcrumb>
  )
}

export default React.memo(AppBreadcrumb)
