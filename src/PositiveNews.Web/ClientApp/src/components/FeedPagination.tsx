import { useCallback, useEffect, useState } from 'react'

type FeedPaginationProps = {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
  className?: string
  listClassName?: string
}

function clampPage(value: number, totalPages: number) {
  return Math.min(Math.max(1, value), totalPages)
}

export function FeedPagination({
  currentPage,
  totalPages,
  onPageChange,
  className,
  listClassName = '',
}: FeedPaginationProps) {
  const [pageInput, setPageInput] = useState(String(currentPage))

  useEffect(() => {
    setPageInput(String(currentPage))
  }, [currentPage])

  const commitPageInput = useCallback(() => {
    const parsed = Number.parseInt(pageInput.trim(), 10)
    const next = Number.isFinite(parsed) ? clampPage(parsed, totalPages) : currentPage
    setPageInput(String(next))
    if (next !== currentPage) {
      onPageChange(next)
    }
  }, [pageInput, totalPages, currentPage, onPageChange])

  if (totalPages <= 1) {
    return null
  }

  const ulClassName = ['pagination', 'feed-pagination', 'mb-0', listClassName].filter(Boolean).join(' ')

  return (
    <nav className={['feed-pagination-nav', className].filter(Boolean).join(' ')} aria-label="Article feed pagination">
      <ul className={ulClassName}>
        <li className={`page-item ${currentPage === 1 ? 'disabled' : ''}`}>
          <button
            type="button"
            className="page-link"
            disabled={currentPage === 1}
            onClick={() => onPageChange(currentPage - 1)}
          >
            Previous
          </button>
        </li>

        <li className="page-item">
          <span className="page-link feed-pagination__status">
            <span>Page</span>
            <input
              type="text"
              inputMode="numeric"
              className="feed-pagination__page-input"
              aria-label="Current page"
              value={pageInput}
              onChange={(e) => setPageInput(e.target.value)}
              onBlur={commitPageInput}
              onKeyDown={(e) => {
                if (e.key === 'Enter') {
                  e.preventDefault()
                  commitPageInput()
                }
              }}
            />
            <span>of {totalPages}</span>
          </span>
        </li>

        <li className={`page-item ${currentPage === totalPages ? 'disabled' : ''}`}>
          <button
            type="button"
            className="page-link"
            disabled={currentPage === totalPages}
            onClick={() => onPageChange(currentPage + 1)}
          >
            Next
          </button>
        </li>
      </ul>
    </nav>
  )
}
