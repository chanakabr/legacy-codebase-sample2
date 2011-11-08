<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:tv="http://www.rss-tv.org/rss/tv1.0"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt" exclude-result-prefixes="msxsl tv"
>
  <xsl:output method="xml" omit-xml-declaration="yes" indent="yes" encoding="utf-8"/>
  <xsl:param name="tvmChannel"></xsl:param>
  <xsl:param name="udid"></xsl:param>
  <xsl:template match="service">
    <xsl:element name="rss">
      <!-- a small hack to show the xmlns:tv-->
      <xsl:attribute name="tv:tv"></xsl:attribute>
      <xsl:attribute name="version">2.0</xsl:attribute>
      <channel>
        <title>Welcome to TVinci</title>
        <promochannel>-</promochannel>
        <nohd>true</nohd>
        <bypass>true</bypass>
        <item>
          <title>Home</title>
          <link tv:type="rss">
            <xsl:text>/tvpapi/gateways/gateway.ashx?type=channel&amp;devtype=AccedoCTV&amp;deviceId=</xsl:text>
            <xsl:value-of select="$udid"/>
          </link>
        </item>
        <item>
          <title>Coming soon</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?xsl=fetchtv/dynDP&amp;id=tvmenu/home/home</link>
        </item>
        <item>
          <title>Highlights</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?action=cms&amp;cms=archive_movie&amp;id=585</link>
        </item>
        <item>
          <title>A-Z Listing</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?xsl=fetchtv/dynDP&amp;id=tvmenu/home/azlisting</link>
        </item>
        <item>
          <title>Search</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?xsl=fetchtv/dynDP&amp;id=tvmenu/home/search</link>
        </item>
        <item>
          <title>Help</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?xsl=fetchtv/dynDP&amp;id=tvmenu/home/help</link>
        </item>
        <item>
          <title>About</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?xsl=fetchtv/dynDP&amp;id=tvmenu/home/about</link>
        </item>
        <item>
          <title>Settings</title>
          <link tv:type="rss">http://ipvision2.rsstv.entriq.net/menu/?xsl=fetchtv/dynDP&amp;id=tvmenu/home/settings</link>
        </item>
      </channel>
    </xsl:element>
  </xsl:template>
  <xsl:template match="GetAllChannels">
    <xsl:element name="rss">
      <!-- a small hack to show the xmlns:tv-->
      <xsl:attribute name="tv:tv"></xsl:attribute>
      <xsl:attribute name="version">2.0</xsl:attribute>
      <xsl:element name="channel">
        <xsl:element name="title">
          <xsl:text>Tvinci VOD</xsl:text>
        </xsl:element>
        <xsl:element name="tv:widget">
          <xsl:attribute name="height">200</xsl:attribute>
          <xsl:attribute name="width">300</xsl:attribute>
          <xsl:text>Photo</xsl:text>
        </xsl:element>
        <xsl:element name="baseURL">http://ibc.cdngc.net/Ipvision/pics/</xsl:element>
        <xsl:element name="tv:basehref">
          http://test.tvinci.com/tvpapi/gateways/gateway.ashx?type=channel&amp;devtype=AccedoCTV&amp;deviceId=<xsl:value-of select="$udid"/>
        </xsl:element>
        <xsl:for-each select="Channel">
          <xsl:element name="item">
            <xsl:element name="title">
              <xsl:value-of select="Title"/>
            </xsl:element>
            <xsl:element name="image">
              <xsl:attribute name="type">Photo</xsl:attribute>
              <xsl:element name="url">http://ibc.cdngc.net/Ipvision/pics/liked.jpg</xsl:element>
            </xsl:element>
            <xsl:element name="link">
              <xsl:attribute name="tv:type">rss</xsl:attribute>
              <xsl:text>/tvpapi/gateways/gateway.ashx?type=channelInfo&amp;intChid=</xsl:text>
              <xsl:value-of select="Tvmch"/>&amp;devtype=AccedoCTV&amp;deviceId=<xsl:value-of select="$udid"/>
            </xsl:element>
          </xsl:element>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="GetChannelInfo">
    <xsl:element name="rss">
      <!-- a small hack to show the xmlns:tv-->
      <xsl:attribute name="tv:tv"></xsl:attribute>
      <xsl:attribute name="version">2.0</xsl:attribute>
      <xsl:element name="channel">
        <xsl:element name="title">
          <xsl:value-of select="Title"/>
        </xsl:element>
        <xsl:element name="baseURL">http://ibc.cdngc.net/Ipvision/pics/</xsl:element>
        <xsl:element name="tv:basehref">
          http://test.tvinci.com/tvpapi/gateways/gateway.ashx?type=channelInfo&amp;devtype=AccedoCTV&amp;deviceId=<xsl:value-of select="$udid"/>
        </xsl:element>
        <tv:style />
        <language />
        <item>
          <title>
            <xsl:value-of select="Title"/>
          </title>
          <indirecturl />
          <indirect />
          <xsl:element name="link">
            <xsl:attribute name="tv:type">rss</xsl:attribute>
            <xsl:text>/tvpapi/gateways/gateway.ashx?type=channelMedias&amp;intChid=</xsl:text>
            <xsl:value-of select="$tvmChannel"/>&amp;devtype=AccedoCTV&amp;deviceId=<xsl:value-of select="$udid"/>
          </xsl:element>
        </item>
      </xsl:element>
    </xsl:element>
  </xsl:template>
  <xsl:template match="GetChannelMedias">
    <xsl:element name="rss">
      <!-- a small hack to show the xmlns:tv-->
      <xsl:attribute name="tv:tv"></xsl:attribute>
      <xsl:attribute name="version">2.0</xsl:attribute>
      <systemURL>
        <xsl:text>http://test.tvinci.com/tvpapi/gateways/gateway.ashx?type=channelMedias&amp;chid=327167&amp;devtype=AccedoCTV&amp;deviceId=</xsl:text>
        <xsl:value-of select="$udid"/>
      </systemURL>
      <xsl:element name="tv:results">
        <xsl:attribute name="total">
          <xsl:value-of select="count(Media)" />
        </xsl:attribute>
        <xsl:attribute name="nextoffset">
          0
        </xsl:attribute>
      </xsl:element>
      <channel tv:cache="180">
        <source >
          <xsl:attribute name="url">
            <xsl:text>http://test.tvinci.com/tvpapi/gateways/gateway.ashx?type=channelMedias&amp;chid=327167&amp;devtype=AccedoCTV&amp;deviceId=</xsl:text>
            <xsl:value-of select="$udid"/>
          </xsl:attribute>
        </source>
        <title>Top 10 Comedies</title>
        <tv:style>
          css:http://media.fetchtv.net/content/
        </tv:style>
        <tv:widget height="300" width="200">BoxArt</tv:widget>
        <xsl:for-each select="Media">
          <item>
            <title>
              <xsl:value-of select="Title"/>
            </title>
            <tv:categoryid>200,101,223</tv:categoryid>
            <description>
              <xsl:value-of select="Description"/>
            </description>
            <image type="BoxArt">
              <url>
                <xsl:value-of select="PicURL"/>
              </url>
              <width>200</width>
              <height>300</height>
            </image>
            <pubDate>2011-11-08T10:11:10</pubDate>
            <nohd>true</nohd>
            <promovideo></promovideo>
            <tv:category>Comedy</tv:category>
            <tv:duration>
              <xsl:value-of select="Duration"/>
            </tv:duration>
            <guid>
              <xsl:value-of select="MediaID"/>
            </guid>
            <tv:guid>
              <xsl:text>guid.</xsl:text>
              <xsl:value-of select="MediaID"/>
            </tv:guid>
            <video length="9239" date="2011-06-21T14:09:47" d4p1:delivery="download" xmlns:d4p1="tv">
              <xsl:attribute name="url">
                <xsl:value-of select="URL"/>
              </xsl:attribute>
            </video>
            <preview length="9654234" duration="124">
              <xsl:attribute name="url">
                <xsl:value-of select="TrailerURL"/>
              </xsl:attribute>
            </preview>
            <tv:meta type="service" display="Service">TVinci</tv:meta>
            <tv:meta type="servicelogo" display="ServiceLogo">TVinci</tv:meta>
            <tv:meta type="year" display="Year">
              <xsl:value-of select="ProductionYear"/>
            </tv:meta>
            <tv:meta type="rightsowner" display="Copyright">
              <xsl:value-of select="Copyright"/>
            </tv:meta>
            <tv:meta type="releasingentity" display="Releasing Entity"></tv:meta>
            <tv:meta type="pgrate" display="Rating Age">
              <xsl:value-of select="Rating"/>
            </tv:meta>
            <tv:meta type="rating" display="Rating">0</tv:meta>
            <tv:meta type="pageTitle" display="">Top 10 Comedies</tv:meta>
            <tv:meta type="lastModified" display="Last Modified">2009-07-14T12:00:00</tv:meta>
            <tv:meta type="director" display="Director">
              <xsl:for-each select="Directors/Name">
                <xsl:value-of select="."/>
              </xsl:for-each>
            </tv:meta>
            <tv:meta type="quality" display="Quality">Normal</tv:meta>
            <tv:meta type="policy" display="Policy">599</tv:meta>
            <tv:meta type="contentid" display="ContentId">
              <xsl:value-of select="MediaID"/>
            </tv:meta>
            <tv:meta type="length" display="Length">923958594</tv:meta>
            <tv:meta type="language" display="Language">English</tv:meta>
          </item>
        </xsl:for-each>
      </channel>
    </xsl:element>
  </xsl:template>
</xsl:stylesheet>
