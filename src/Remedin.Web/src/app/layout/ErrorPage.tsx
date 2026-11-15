import { Link } from 'react-router-dom'

import { Notice } from '@/shared/ui/Notice/Notice'

export function ErrorPage() {
  return (
    <div>
      <h1>Algo deu errado</h1>

      <Notice tone="danger">
        Não foi possível carregar esta tela. Tente de novo em instantes.
      </Notice>

      <p>
        <Link to="/">Voltar para a busca</Link>
      </p>
    </div>
  )
}
