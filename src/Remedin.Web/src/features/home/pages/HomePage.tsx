import type { ReactNode } from 'react'

import { HomeHero } from '../components/HomeHero'
import { PriceStory } from '../components/PriceStory'
import { useExample } from '../model/useExample'

import styles from './HomePage.module.css'

export function HomePage({ search }: { search: ReactNode }) {
  const example = useExample()

  return (
    <div className={styles.home}>
      <HomeHero search={search} example={example} />
      <PriceStory example={example} />
    </div>
  )
}
