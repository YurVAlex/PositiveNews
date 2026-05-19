import { Link } from 'react-router-dom'

type ArticleTopicLinksProps = {
  topics: string[]
  selectedTopics: string[]
  buildTopicToggleUrl: (topic: string) => string
}

function isTopicSelected(topic: string, selectedTopics: string[]): boolean {
  const lower = topic.toLowerCase()
  return selectedTopics.some((t) => t.toLowerCase() === lower)
}

export function ArticleTopicLinks({ topics, selectedTopics, buildTopicToggleUrl }: ArticleTopicLinksProps) {
  const distinct = [...new Set(topics ?? [])].filter((t) => t.trim().length > 0)
  if (distinct.length === 0) {
    return null
  }

  return (
    <div className="d-flex flex-wrap gap-1 mb-2">
      {distinct.map((topic) => {
        const isActive = isTopicSelected(topic, selectedTopics)
        const btnClass = isActive ? 'btn btn-primary btn-sm' : 'btn btn-outline-dark btn-sm'

        return (
          <Link
            key={topic}
            to={buildTopicToggleUrl(topic)}
            className={btnClass}
            style={{ fontSize: '0.85rem' }}
            title={isActive ? `Remove "${topic}" from preferred topics` : `Prefer articles about "${topic}"`}
          >
            {topic}
            {isActive ? (
              <span className="ms-1 opacity-75" aria-hidden="true">
                ×
              </span>
            ) : null}
          </Link>
        )
      })}
    </div>
  )
}
