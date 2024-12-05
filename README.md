<h1>HealthCare App</h1>
<p>En applikation för att hantera tillgänglighet och bokningar inom vården.</p>
<h2>Klona repot</h2>
    <p>Projektet är uppdelat i två delar:</p>
    <ul>
        <li><strong>HealthCareAPI</strong></li>
        <li><strong>HealthCareApp</strong></li>
    </ul>
    
<p>git clone https://github.com/s1ggelin/HealthCareAPI.git</p>
<p>git clone https://github.com/s1ggelin/HealthCareApp.git</p>

<h2>Konfigurera projektet</h2>
    <ol>
        <li>Öppna lösningen i Visual Studio.</li>
        <li>Gå till <strong>solution -> Add -> Existing project</strong>.</li>
        <li>Lägg till både <code>HealthCareAPI</code> och <code>HealthCareWebb</code> i solution foldern.</li>
        <li>Gå till <strong>Project -> Configure Startup Projects</strong>.</li>
        <li>Lägg till både <code>HealthCareAPI</code> och <code>HealthCareWebb</code> som startprojekt.</li>
    </ol>

 <h2>Postgres-databas</h2>
    <p>Kör följande kommandon i Package Manager Console:</p>
    
<p>Add-Migration</p>
<p>Update-Database</p>

 <h2>Starta applikationen</h2>
    <p>Starta projekten via Visual Studio. Både API och webbapplikationen kommer att köras lokalt.</p>
