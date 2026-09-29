import { Link } from 'react-router-dom'

import { Notice } from '@/shared/ui/Notice/Notice'

/**
 * Tela mostrada quando a renderização de uma rota falha.
 *
 * Sem ela, um erro em qualquer componente derruba a aplicação inteira e deixa
 * a página em branco, sem nada que explique o que houve.
 */
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
