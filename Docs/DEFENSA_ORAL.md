# Defensa oral - 3 a 5 minuts

Guio orientatiu en catala. Practica'l amb el projecte obert i explica el codi amb les teves paraules.

## 0:00-0:35 - Presentacio i demostracio

"He creat el nucli jugable de Coin Collector 3D amb Unity 6.6 i C#. El jugador controla una esfera que es mou amb fisica i recull sis monedes. El projecte te dues escenes: MenuPrincipal i Level1."

Nota de versio: l'enunciat demana Unity 6 LTS; aquest projecte s'ha executat i comprovat amb Unity 6.6 (6000.6.2f1). Explica aquesta diferencia amb transparencia.

Mostra el menu, prem Play i mou la bola amb WASD o les fletxes. Recull una moneda i mostra a la Console `Coin collected! Score: 1`.

## 0:35-1:25 - Moviment del jugador

Selecciona Player a la Hierarchy i mostra Rigidbody, SphereCollider, el tag Player i Freeze Rotation X/Z. Obre PlayerController.cs.

"Llegeixo els eixos Horizontal i Vertical a Update. Limito el vector d'entrada a longitud 1 perque el moviment diagonal no sigui mes rapid. A FixedUpdate assigno la velocitat horitzontal al Rigidbody amb linearVelocity. Mantinc la component Y per respectar la gravetat. La velocitat speed es privada, pero es pot configurar a l'Inspector amb SerializeField."

Explica que FixedUpdate segueix els passos de simulacio fisica i que la interpolacio del Rigidbody suavitza la representacio visual entre passos. No mous la bola canviant transform.position cada frame.

## 1:25-2:15 - Monedes i puntuacio

Obre Coin.prefab i Coin.cs.

"Cada moneda es una instancia del mateix prefab, amb material daurat i SphereCollider configurat com a trigger. OnTriggerEnter comprova el tag Player. Si es el jugador, afegeix un punt al GameManager, escriu el missatge exacte a la Console i destrueix la moneda. El boolea collected evita comptar dues vegades abans que Destroy s'apliqui al final del frame."

Obre GameManager.cs.

"GameManager segueix el patro Singleton: Instance permet que les monedes i la KillZone trobin el gestor. Awake elimina instancies duplicades. score es privat; AddScore modifica la puntuacio i Score nomes permet llegir-la. El gestor pertany al nivell, per tant en reiniciar es crea una instancia nova amb zero punts."

## 2:15-3:05 - Nivell, triggers i reinici

Mostra Ground, InvisibleWalls_Triggers i KillZone. Selecciona una paret per veure MeshRenderer desactivat i BoxCollider amb Is Trigger.

"El terra es un Plane amb tag Ground. Les quatre parets son cubs invisibles configurats com a trigger, com demana l'enunciat. Com que un trigger no frena un cos, BoundaryWall mante el jugador a l'interior corregint la penetracio i anul·lant nomes la velocitat cap a fora."

Prem F i mostra `Player killed!`, la reaparicio i les sis monedes restaurades.

"F es un atall de demostracio: deixa la bola fora del Plane i la gravetat la fa caure. El cub KillZone esta sota del nivell; quan detecta el jugador crida RestartLevel. Aquest metode carrega l'index de l'escena activa amb SceneManager.LoadScene."

## 3:05-3:40 - Ampliacions i organitzacio

Mostra Spinner.cs i CoinFloat.cs.

"He fet les dues ampliacions opcionals. Spinner gira la moneda a l'eix Y utilitzant Time.deltaTime. CoinFloat guarda la posicio inicial i suma un desplacament vertical calculat amb Mathf.PingPong. Cada responsabilitat esta en un component petit."

Mostra les carpetes Assets/Scenes, Scripts, Prefabs i Materials. Ensenya les dues escenes habilitades a Build Profiles.

## 3:40-4:15 - GitHub i proves

"El repositori conte Assets, Packages i ProjectSettings, incloent els fitxers meta que mantenen les referencies entre scripts, materials, prefabs i escenes. El gitignore exclou carpetes generades com Library i Temp. El README explica els controls, la instal·lacio i la correspondencia entre els requisits i els fitxers."

Explica les proves que constin realment a VERIFICACION.md. Diferencia les comprovacions de codi, les d'estructura i les de Play Mode. No afirmis haver executat proves que encara estiguin pendents.

## Preguntes habituals

- **Per que OnTriggerEnter i no OnCollisionEnter?** Perque la moneda i la KillZone son triggers: detecten solapaments sense produir una resposta de col·lisio solida.
- **Per que conservar Y?** Per no anul·lar la gravetat ni una velocitat vertical existent quan apliquem el moviment horitzontal.
- **Que passa si no normalitzem la diagonal?** Els eixos X i Z poden valer 1 alhora i la magnitud passa a arrel de 2.
- **Per que no faig score public?** Per encapsular l'estat; els altres components fan servir AddScore i nomes poden consultar Score.
- **Per que no DontDestroyOnLoad al GameManager?** Aquest lliurament necessita reiniciar els punts quan es torna a carregar Level1.
- **Que te una instancia de prefab?** Comparteix la configuracio de l'asset Coin.prefab i nomes canvia la posicio i el nom a l'escena.
- **Que fa Is Trigger?** Canvia una barrera fisica per una deteccio de solapament. El Rigidbody del jugador permet generar els callbacks.
- **Com es veu la puntuacio?** A la Console i al camp score de l'Inspector. La UI de partida queda per als lliuraments posteriors.
