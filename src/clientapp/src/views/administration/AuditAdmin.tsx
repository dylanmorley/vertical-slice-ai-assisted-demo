import React, { useState, useEffect } from 'react'
import {
  CCard,
  CCardBody,
  CCardHeader,
  CTable,
  CTableBody,
  CTableDataCell,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CSpinner,
  CAlert,
  CFormInput,
  CRow,
  CCol,
  CBadge,
  CPagination,
  CPaginationItem,
  CFormSelect,
  CButton,
  CInputGroup,
  CInputGroupText,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilSearch, cilShieldAlt, cilClock, cilUser, cilWarning } from '@coreui/icons'
import { useVerticalSliceClient } from '../../api'
import type { AuditSummary, GetAuditRecordsParams as AuditQueryParams } from '../../api'

const AuditAdmin = () => {
  const apiClient = useVerticalSliceClient()

  const [audits, setAudits] = useState<AuditSummary[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [currentPage, setCurrentPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalItems, setTotalItems] = useState(0)
  const [pageSize, setPageSize] = useState(50)

  // Filter states
  const [searchTerm, setSearchTerm] = useState('')
  const [operationFilter, setOperationFilter] = useState('')
  const [entityTypeFilter, setEntityTypeFilter] = useState('')
  const [userFilter, setUserFilter] = useState('')
  const [successFilter, setSuccessFilter] = useState<boolean | null>(null)

  useEffect(() => {
    loadAudits()
  }, [
    currentPage,
    pageSize,
    searchTerm,
    operationFilter,
    entityTypeFilter,
    userFilter,
    successFilter,
  ])

  const loadAudits = async () => {
    try {
      setLoading(true)
      setError(null)

      const params: AuditQueryParams = {
        page: currentPage,
        pageSize: pageSize,
        searchTerm: searchTerm || undefined,
        operation: operationFilter || undefined,
        entityType: entityTypeFilter || undefined,
        userName: userFilter || undefined,
        isSuccess: successFilter !== null ? successFilter : undefined,
      }

      const response = await apiClient.audit.getRecords(params)

      setAudits(response.data)
      setTotalPages(response.pagination.totalPages)
      setTotalItems(response.pagination.totalItems)
    } catch (err) {
      console.error('Failed to load audit records:', err)
      setError('Failed to load audit records')
    } finally {
      setLoading(false)
    }
  }

  const handlePageChange = (page: number) => {
    setCurrentPage(page)
  }

  const handlePageSizeChange = (newPageSize: number) => {
    setPageSize(newPageSize)
    setCurrentPage(1) // Reset to first page when changing page size
  }

  const clearFilters = () => {
    setSearchTerm('')
    setOperationFilter('')
    setEntityTypeFilter('')
    setUserFilter('')
    setSuccessFilter(null)
    setCurrentPage(1)
  }

  const formatTimestamp = (timestamp: string) => {
    return new Date(timestamp).toLocaleString()
  }

  const getOperationBadgeColor = (operation: string) => {
    switch (operation.toUpperCase()) {
      case 'CREATE':
        return 'success'
      case 'UPDATE':
        return 'warning'
      case 'DELETE':
        return 'danger'
      case 'GET':
        return 'info'
      default:
        return 'secondary'
    }
  }

  const getSuccessBadgeColor = (isSuccess: boolean) => {
    return isSuccess ? 'success' : 'danger'
  }

  const renderPagination = () => {
    if (totalPages <= 1) return null

    const items = []
    const maxVisiblePages = 5
    let startPage = Math.max(1, currentPage - Math.floor(maxVisiblePages / 2))
    const endPage = Math.min(totalPages, startPage + maxVisiblePages - 1)

    if (endPage - startPage + 1 < maxVisiblePages) {
      startPage = Math.max(1, endPage - maxVisiblePages + 1)
    }

    // Previous button
    items.push(
      <CPaginationItem
        key="prev"
        disabled={currentPage === 1}
        onClick={() => handlePageChange(currentPage - 1)}
      >
        Previous
      </CPaginationItem>,
    )

    // Page numbers
    for (let i = startPage; i <= endPage; i++) {
      items.push(
        <CPaginationItem key={i} active={i === currentPage} onClick={() => handlePageChange(i)}>
          {i}
        </CPaginationItem>,
      )
    }

    // Next button
    items.push(
      <CPaginationItem
        key="next"
        disabled={currentPage === totalPages}
        onClick={() => handlePageChange(currentPage + 1)}
      >
        Next
      </CPaginationItem>,
    )

    return <CPagination>{items}</CPagination>
  }

  if (loading && audits.length === 0) {
    return (
      <CCard>
        <CCardBody className="text-center">
          <CSpinner size="lg" />
          <p className="mt-3">Loading audit records...</p>
        </CCardBody>
      </CCard>
    )
  }

  return (
    <div>
      <CCard>
        <CCardHeader>
          <div className="d-flex justify-content-between align-items-center">
            <div className="d-flex align-items-center">
              <CIcon icon={cilShieldAlt} className="me-2" />
              <h5 className="mb-0">Audit Records</h5>
            </div>
            <CBadge color="info" className="fs-6">
              {totalItems.toLocaleString()} total records
            </CBadge>
          </div>
        </CCardHeader>
        <CCardBody>
          {error && (
            <CAlert color="danger" className="mb-3">
              {error}
            </CAlert>
          )}

          {/* Filters */}
          <CRow className="mb-3">
            <CCol md={3}>
              <CFormInput
                type="text"
                placeholder="Search..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
              />
            </CCol>
            <CCol md={2}>
              <CFormSelect
                value={operationFilter}
                onChange={(e) => setOperationFilter(e.target.value)}
              >
                <option value="">All Operations</option>
                <option value="CREATE">CREATE</option>
                <option value="UPDATE">UPDATE</option>
                <option value="DELETE">DELETE</option>
                <option value="GET">GET</option>
              </CFormSelect>
            </CCol>
            <CCol md={2}>
              <CFormSelect
                value={entityTypeFilter}
                onChange={(e) => setEntityTypeFilter(e.target.value)}
              >
                <option value="">All Entities</option>
                <option value="Node">Node</option>
                <option value="Organization">Organization</option>
                <option value="NodeType">NodeType</option>
                <option value="Geography">Geography</option>
              </CFormSelect>
            </CCol>
            <CCol md={2}>
              <CFormSelect
                value={successFilter === null ? '' : successFilter.toString()}
                onChange={(e) => {
                  const value = e.target.value
                  setSuccessFilter(value === '' ? null : value === 'true')
                }}
              >
                <option value="">All Results</option>
                <option value="true">Success</option>
                <option value="false">Failed</option>
              </CFormSelect>
            </CCol>
            <CCol md={2}>
              <CFormInput
                type="text"
                placeholder="User name"
                value={userFilter}
                onChange={(e) => setUserFilter(e.target.value)}
              />
            </CCol>
            <CCol md={1}>
              <CButton color="secondary" variant="outline" onClick={clearFilters} className="w-100">
                Clear
              </CButton>
            </CCol>
          </CRow>

          {/* Page size selector */}
          <CRow className="mb-3">
            <CCol md={2}>
              <CFormSelect
                value={pageSize}
                onChange={(e) => handlePageSizeChange(Number(e.target.value))}
              >
                <option value={25}>25 per page</option>
                <option value={50}>50 per page</option>
                <option value={100}>100 per page</option>
              </CFormSelect>
            </CCol>
          </CRow>

          {/* Table */}
          <CTable responsive striped hover>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell>Timestamp</CTableHeaderCell>
                <CTableHeaderCell>Operation</CTableHeaderCell>
                <CTableHeaderCell>Entity</CTableHeaderCell>
                <CTableHeaderCell>User</CTableHeaderCell>
                <CTableHeaderCell>Endpoint</CTableHeaderCell>
                <CTableHeaderCell>Status</CTableHeaderCell>
                <CTableHeaderCell>Duration</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              {audits.map((audit) => (
                <CTableRow key={audit.auditId}>
                  <CTableDataCell>
                    <div className="d-flex align-items-center">
                      <CIcon icon={cilClock} className="me-2 text-muted" />
                      <small>{formatTimestamp(audit.timestamp)}</small>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getOperationBadgeColor(audit.operation)}>
                      {audit.operation}
                    </CBadge>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div>
                      <strong>{audit.entityType}</strong>
                      <br />
                      <small className="text-muted">ID: {audit.entityId}</small>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div className="d-flex align-items-center">
                      <CIcon icon={cilUser} className="me-2 text-muted" />
                      <div>
                        <div>{audit.userName}</div>
                        <small className="text-muted">{audit.userId}</small>
                      </div>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <div>
                      <CBadge color="info" className="me-1">
                        {audit.httpMethod}
                      </CBadge>
                      <small className="text-muted">{audit.endpoint}</small>
                    </div>
                  </CTableDataCell>
                  <CTableDataCell>
                    <CBadge color={getSuccessBadgeColor(audit.isSuccess)}>
                      {audit.isSuccess ? 'Success' : 'Failed'}
                    </CBadge>
                    {audit.errorMessage && (
                      <div className="mt-1">
                        <CIcon icon={cilWarning} className="text-warning me-1" />
                        <small className="text-warning">{audit.errorMessage}</small>
                      </div>
                    )}
                  </CTableDataCell>
                  <CTableDataCell>
                    {audit.durationMs && <span>{audit.durationMs}ms</span>}
                  </CTableDataCell>
                </CTableRow>
              ))}
            </CTableBody>
          </CTable>

          {/* Pagination */}
          <div className="d-flex justify-content-between align-items-center mt-3">
            <div>
              Showing {(currentPage - 1) * pageSize + 1} to{' '}
              {Math.min(currentPage * pageSize, totalItems)} of {totalItems.toLocaleString()}{' '}
              records
            </div>
            {renderPagination()}
          </div>
        </CCardBody>
      </CCard>
    </div>
  )
}

export default AuditAdmin
