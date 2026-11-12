# Compilacion de Windows

- Editor: Unity 6.6 (6000.6.2f1).
- Plataforma: Windows 64 bits, backend Mono.
- Escenas: MenuPrincipal (indice 0) y Level1 (indice 1), ambas habilitadas.
- Resultado: compilacion completada correctamente con `PracticeBuilder.BuildWindows`.
- Ejecutable: `Builds/Windows/CoinCollector3D.exe`.

La compilacion se distribuye en un ZIP separado. El repositorio conserva el proyecto fuente completo y excluye Builds mediante el gitignore. Para volver a compilar, abre el proyecto en el editor y usa **UF2 > Compilar Windows**.

El informe de Play Mode esta en `VERIFICACION.md`; las capturas reales estan en `Screenshots/`. La validacion se ha realizado con Unity 6.6. El requisito original de utilizar LTS se ha sustituido por el uso de la version instalada en el equipo.
