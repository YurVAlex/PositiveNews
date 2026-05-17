import { Link } from 'react-router-dom'

export type FeedActiveTopicsProps = {
  topics: string[]
  buildTopicToggleUrl: (topic: string) => string
  hint?: string | null
  className?: string
}

export function FeedActiveTopics({ topics, buildTopicToggleUrl, hint, className }: FeedActiveTopicsProps) {
  if (topics.length === 0) {
    return null
  }

  return (
    <div className={['alert alert-info mb-3', className].filter(Boolean).join(' ')}>
      {hint ? <div className="mb-2">{hint}</div> : null}
      <div className="d-flex flex-wrap align-items-center gap-2">
        <span className="small text-muted me-1">Active topics:</span>
        {topics.map((t) => (
          <Link
            key={t}
            to={buildTopicToggleUrl(t)}
            className="btn btn-sm btn-primary"
            title={`Remove "${t}" from preferred topics`}
          >
            {t}
            <span className="ms-1 opacity-75" aria-hidden="true">
              ×
            </span>
          </Link>
        ))}
      </div>
    </div>
  )
}
