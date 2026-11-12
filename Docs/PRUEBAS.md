# Plan de pruebas

## Pruebas manuales

1. Abrir MenuPrincipal, iniciar Play Mode y pulsar el boton Play. Debe cargar Level1.
2. Moverse con WASD y con las flechas. Las diagonales deben tener la misma rapidez que los desplazamientos rectos. Soltar las teclas detiene el desplazamiento horizontal.
3. Cambiar Speed en el Inspector durante Play Mode y comprobar que cambia la rapidez. Los cambios hechos en Play Mode se deshacen al salir.
4. Inspeccionar Rigidbody: gravedad activa, interpolacion activa y rotacion X/Z bloqueada. La bola permanece sobre el suelo.
5. Recoger cada moneda: se destruye y la Console muestra `Coin collected! Score: X`, desde 1 hasta 6. Con Collapse desactivado se ven los seis mensajes.
6. Verificar que las monedas giran sobre Y y flotan verticalmente.
7. Recorrer los cuatro bordes y las esquinas. Las paredes son invisibles y mantienen al jugador dentro del nivel.
8. Pulsar F. La bola cae fuera del Plane, entra en KillZone y la Console muestra `Player killed!`. Level1 vuelve a cargarse con seis monedas y score igual a cero.
9. Recoger monedas y pulsar R. Deben restaurarse la puntuacion y las monedas.
10. Pulsar Esc. Debe volver al menu y su boton Play debe permitir comenzar otra partida.
11. Abrir File > Build Profiles y comprobar que MenuPrincipal y Level1 estan habilitadas y en ese orden.
12. Comprobar que la Console no contiene errores rojos ni referencias perdidas.

## Verificacion automatica de integracion

`Assets/Editor/PracticeVerificationRunner.cs` inicia Play Mode desde el menu y crea un verificador temporal. `Assets/Tests/PracticeVerificationDriver.cs` se compila solo para el editor.

Comprueba el evento persistente de Play, cambio de escena, tags, colliders, restricciones del Rigidbody, cuatro paredes trigger, seis monedas, conservacion de Y, desplazamiento fisico, animaciones, rechazo de objetos sin tag Player, recogida y destruccion de las seis monedas, mensajes exactos, Singleton duplicado, contencion en el borde, caida real y restauracion del nivel. La prueba usa el motor fisico, no invoca manualmente OnTriggerEnter.

Ejemplo desde PowerShell, sustituyendo la ruta del editor y del proyecto por las de tu equipo:

```powershell
& 'C:\Program Files\Unity 6000.6.2f1\Editor\Unity.exe' `
    -batchmode -projectPath 'C:\ruta\coin-collector-3d-uf2' `
    -executeMethod PracticeVerificationRunner.Run `
    -logFile 'C:\ruta\verificacion.log'
```

No se utiliza `-nographics`, porque tambien se capturan imagenes del menu y del nivel. No se utiliza `-quit`, porque el verificador debe esperar a que terminen las pruebas de Play Mode. Al finalizar escribe `Docs/VERIFICACION.md` y cierra el editor con codigo 0 (correcto) o 1 (fallo). Hay que cerrar antes otras instancias del mismo proyecto.

## Alcance de cada comprobacion

Una compilacion externa de C# confirma sintaxis y compatibilidad con las API referenciadas. No demuestra que una escena importe correctamente ni que los triggers se ejecuten. Solo debe considerarse superada la prueba de integracion cuando el informe indique PASS tras ejecutarse dentro de Unity.
