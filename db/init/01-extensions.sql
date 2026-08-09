-- unaccent: busca sem acento ("acido" acha "ÁCIDO").
-- pg_trgm: similaridade por trigrama, para tolerar erro de digitação.
CREATE EXTENSION IF NOT EXISTS unaccent;
CREATE EXTENSION IF NOT EXISTS pg_trgm;

-- unaccent é STABLE por padrão, o que impede usá-la em índice. Esta função
-- envelope é IMMUTABLE e torna o índice de texto possível.
CREATE OR REPLACE FUNCTION immutable_unaccent(text)
RETURNS text
LANGUAGE sql IMMUTABLE STRICT PARALLEL SAFE
AS $$ SELECT public.unaccent('public.unaccent'::regdictionary, $1) $$;
