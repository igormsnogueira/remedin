import { Link } from 'react-router-dom'

import { useDocumentTitle } from '@/shared/hooks/useDocumentTitle'
import { Notice } from '@/shared/ui/Notice/Notice'

export function NotFoundPage() {
  useDocumentTitle('Página não encontrada')

  return (
    <div>
      <h1>Página não encontrada</h1>

      <Notice>
        O endereço que você abriu não existe. Pode ser um link antigo ou um erro de digitação.
      </Notice>

      <p>
        <Link to="/">Voltar para a busca</Link>
      </p>
    </div>
  )
}
