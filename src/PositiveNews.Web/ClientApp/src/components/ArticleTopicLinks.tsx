import { Link } from 'react-router-dom'

type ArticleTopicLinksProps = {
  topics: string[]
  selectedTopic: string | null | undefined
}

export function ArticleTopicLinks({ topics, selectedTopic }: ArticleTopicLinksProps) {
  const distinct = [...new Set(topics ?? [])].filter((t) => t.trim().length > 0)
  if (distinct.length === 0) {
    return null
  }

  return (
    <div className="d-flex flex-wrap gap-1 mb-2">
      {distinct.map((topic) => {
        const isActive =
          selectedTopic != null && topic.toLowerCase() === selectedTopic.toLowerCase()
        const btnClass = isActive ? 'btn btn-primary btn-sm' : 'btn btn-outline-dark btn-sm'
        const qs = new URLSearchParams({ topic, page: '1' })

        return (
          <Link key={topic} to={`/?${qs.toString()}`} className={btnClass} style={{ fontSize: '0.85rem' }}>
            {topic}
          </Link>
        )
      })}
    </div>
  )
}
