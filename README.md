eVR
===

Electronic Vehicle Registration

De nieuwe kentekenkaart (eVR) bevat per 1 januari 2014 een chip waarop informatie omtrent voertuig en houder is opgenomen conform EU regelgeving. 

De RDW stelt demo software "as is"  beschikbaar in open source formaat (BSD license) beschikbaar om de kaart uit te kunnen lezen en op een aantal beveiligingskenmerken te verifieren.

Het document "DA12 - Kaart Uitleesdocumentatie" kunt u gebruiken om de Interface specificatie te bestuderen om de kentekenkaart applet uit te lezen en kan worden geverifieerd.
 
De Nederlandse kentekenkaart voldoet aan de Europese richtlijn 2003/123/EC [1]. De chip bevat de data zoals gespecificeerd in de richtlijn en beveiligd zoals beschreven in de richtlijn. Ook bevat de chip in de Nederlandse kentekenkaart additionele registratiedata en kan deze Certificaat van Oorsprong (CVO) data bevatten. De chip van de Nederlandse kentekenkaart is daarnaast voorzien van twee additionele beveiligingsmechanismen. Alle data op de kentekenkaart chip is beveiligd door middel van Passive Authentication (PA) [2]. Dit houdt in dat over alle data op de chip een elektronische handtekening is gezet door de uitgevende instantie, RDW, die ervoor zorgt dat gecontroleerd kan worden dat de data in de verschillende datagroepen ongewijzigd is en bij elkaar hoort. Daarnaast kan de echtheid van de chip gecontroleerd worden door middel van Active Authentication (AA) [2]. Dit betekent dat de chip zijn echtheid kan aantonen door middel van een challenge-response protocol. 

Er wordt van uitgegaan dat de gebruiker bekend is met asymmetrische cryptografie en public key infrastructuren.

LET OP!
I.v.m. problemen met het uitlezen van de CVO data op kentekenkaarten uitgegeven vanaf begin september 2021 is CVO tijdelijk uitgeschakeld. 
Er wordt gewerkt aan een nieuwe versie om dit weer inzichtelijk te krijgen.
Deze versie inclusief demo project is geupdate en werkt tegen alle kentekenkaarten (ook die met een nieuwe chip)