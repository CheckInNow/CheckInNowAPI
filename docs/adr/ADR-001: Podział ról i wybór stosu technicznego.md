# ADR-001: Organizacja pracy i dokumentacji projektu

- **Status:** Proposed
- **Data:** 2026-10-19
- **Decydenci:** Zespół projektowy

## Kontekst

Na początku realizacji projektu ustaliliśmy podstawowe elementy organizacyjne takie jak:

- stos technologiczny;
- role w projekcie;
- nazwę projektu;
- GitHub Projects jako narzędzie do zarządzania zadaniami zamiast Jiry;
- utworzyliśmy osobne repozytoria dla frontendu i backendu.

Potrzebujemy wspólnego sposobu dokumentowania decyzji, planowania prac i określania wymagań funkcjonalnych przed rozpoczęciem właściwej implementacji.

## Podział ról w projekcie

[Julia](https://github.com/1JuliaKozlowska) - project manager / backend  
[Stanislav](https://github.com/stanislavzhukk) - backend / tester / devops  
[Paweł](https://github.com/PawelMiedzinski) - frontend / tester  

## Rozważane rozwiązania

1. GitHub Projects — zarządzanie zadaniami w ekosystemie GitHub.
2. Backend zrealizowany w .NET 10.

## Stos techniczny

**Frontend**
- React
- Vite
- TypeScript
- Tailwind CSS

**Backend**
- .NET 10
- C# 14
- .NET Aspire 13
- Entity Framework Core 10
- xUnit v3
- Redis
- PostgreSQL
- Mailgun
- Mailpit

## Uruchamianie i wdrażanie

Projekt będzie uruchamiany przy użyciu najnowszej wersji Dockera.

Przewidywane środowisko wdrożeniowe obejmuje:
- Cloudflare
- Microsoft Azure

## Decyzja

Przyjmujemy GitHub Projects jako główne narzędzie do zarządzania zadaniami i backlogiem projektu.  Każda planowana praca, zadanie implementacyjne lub istotna aktywność projektowa powinna być reprezentowana przez GitHub Issue z odpowiednimi etykietami.

Dokumentacja projektowa będzie rozwijana wraz z postępem prac. Decyzje architektoniczne będą zapisywane w formacie ADR, a wymagania produktowe zostaną opisane w PRD.
