/**
 * Banner listing currently preferred sources; clicking a chip removes it from the filter.
 */
import { Link } from 'react-router-dom'
import type { SourceFilterItem } from '../api/types'

export type FeedActiveSourcesProps = {
  sources: SourceFilterItem[]
  buildSourceToggleUrl: (sourceId: number) => string
  hint?: string | null
  className?: string
}

export function FeedActiveSources({ sources, buildSourceToggleUrl, hint, className }: FeedActiveSourcesProps) {
  if (sources.length === 0) {
    return null
  }

  return (
    <div className={['alert alert-info mb-3', className].filter(Boolean).join(' ')}>
      {hint ? <div className="mb-2">{hint}</div> : null}
      <div className="d-flex flex-wrap align-items-center gap-2">
        <span className="small text-muted me-1">Active sources:</span>
        {sources.map((s) => (
          <Link
            key={s.id}
            to={buildSourceToggleUrl(s.id)}
            className="btn btn-sm btn-primary d-inline-flex align-items-center gap-1"
            title={`Remove "${s.name}" from preferred sources`}
          >
            {s.logoUrl ? (
              <img
                src={s.logoUrl}
                alt=""
                width={20}
                height={20}
                style={{ objectFit: 'cover' }}
              />
            ) : null}
            {s.name}
            <span className="ms-1 opacity-75" aria-hidden="true">
              ×
            </span>
          </Link>
        ))}
      </div>
    </div>
  )
}
