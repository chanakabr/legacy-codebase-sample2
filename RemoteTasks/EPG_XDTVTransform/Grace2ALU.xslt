<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                exclude-result-prefixes="msxsl" xmlns:myUtils="pda:Utils">
  <xsl:output method="xml" indent="yes" omit-xml-declaration="yes"/>
  <xsl:param name="ID" select="1" />
  <xsl:param name="channelName" select="_" />
  <xsl:param name ="from" select="2000-01-01" />
  <xsl:param name ="to" select="2100-01-01" />
  <xsl:template match="/">
    <xsl:element name="xtvd" xmlns="urn:TMSWebServices">
      <xsl:attribute name="xsi:schemaLocation" namespace="http://www.w3.org/2001/XMLSchema-instance">urn:TMSWebServices http://docs.tms.tribune.com/tech/xml/schemas/tmsxtvd.xsd</xsl:attribute>
      <xsl:attribute name="schemaVersion">1.3</xsl:attribute>
        <xsl:attribute name="from">
          <xsl:value-of select="$from"/>
        </xsl:attribute>
       <xsl:attribute name="to">
          <xsl:value-of select="$to"/>
        </xsl:attribute>
        <xsl:element name="stations">
        <xsl:element name="station">
          <xsl:call-template name="create_station">
            <xsl:with-param name="response" select="."/>
          </xsl:call-template>
          <xsl:element name="name">
            <xsl:value-of select="$channelName" />
          </xsl:element>
        </xsl:element>
      </xsl:element>
      <xsl:element name="schedules">
        <xsl:for-each select="*[local-name()='TVGRIDBATCH']/*[local-name()='GRIDCHANGE']/*[local-name()='TVAIRING']">
          <xsl:call-template name="create_schedule">
            <xsl:with-param name="tvairingCurrent" select="."/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="programs">
        <xsl:for-each select="*[local-name()='TVGRIDBATCH']/*[local-name()='GRIDCHANGE']/*[local-name()='TVPROGRAM']">
          <xsl:call-template name="create_program">
            <xsl:with-param name="program_current" select="."/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:element>
      <xsl:element name="costumFields">
        <xsl:for-each select="*[local-name()='TVGRIDBATCH']/*[local-name()='GRIDCHANGE']/*[local-name()='TVPROGRAM']">
          <xsl:call-template name="create_costumFields">
            <xsl:with-param name="program_current" select="."/>
          </xsl:call-template>
        </xsl:for-each>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="create_station">
    <xsl:param name="response"/>
    <xsl:attribute name="id">
      <xsl:value-of select="$ID"/>
    </xsl:attribute>
    <xsl:element name="callSign" xmlns="urn:TMSWebServices">
      <xsl:value-of select="$ID"/>
    </xsl:element>
  </xsl:template>

  <xsl:template name="create_schedule">
    <xsl:param name="tvairingCurrent"/>
    <xsl:if test="$tvairingCurrent/@GN_ID != ''">
      <xsl:element name="schedule" xmlns="urn:TMSWebServices">
        <xsl:attribute name="program" namespace="">
          <xsl:value-of select="string($tvairingCurrent/@GN_ID)"/>
        </xsl:attribute>
        <xsl:attribute name="station" namespace="">
          <xsl:value-of select="$ID"/>
        </xsl:attribute>
        <xsl:attribute name="time" namespace="">
          <xsl:if test="$tvairingCurrent/@START != ''">
            <xsl:value-of select="concat(string($tvairingCurrent/@START), ':00Z')"/>
          </xsl:if>
        </xsl:attribute>
        <xsl:attribute name="duration" namespace="">
          <xsl:value-of select="myUtils:GetProgramDuration($tvairingCurrent/@START, $tvairingCurrent/@END)"/>
        </xsl:attribute>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="create_program">
    <xsl:param name="program_current"/>
    <xsl:if test="$program_current/*[local-name()='GN_ID'] != ''">
      <xsl:element name ="program" xmlns="urn:TMSWebServices">
        <xsl:attribute name="id" namespace="">
          <xsl:value-of select="string($program_current/*[local-name()='GN_ID'])"/>
        </xsl:attribute>
        <xsl:element name="title">
          <xsl:if test="$program_current/*[local-name()='TITLE'] != ''">
            <xsl:value-of select="string($program_current/*[local-name()='TITLE'])"/>
          </xsl:if>
        </xsl:element>
        <xsl:element name="subtitle">
          <xsl:if test="$program_current/*[local-name()='LISTING'] != ''">
            <xsl:value-of select="string($program_current/*[local-name()='LISTING'])"/>
          </xsl:if>
        </xsl:element>
        <xsl:element name="description">
          <xsl:if test="$program_current/*[local-name()='SYNOPSIS'] != ''">
            <xsl:value-of select="string($program_current/*[local-name()='SYNOPSIS'])"/>
          </xsl:if>
        </xsl:element>
        <xsl:element name="showType">
          <xsl:if test="$program_current/*[local-name()='IPGCATEGORY']/*[local-name()='IPGCATEGORY_L2']/@ID != ''">
            <xsl:value-of select="string(floor(number(string($program_current/*[local-name()='IPGCATEGORY']/*[local-name()='IPGCATEGORY_L2']/@ID))))"/>
          </xsl:if>
        </xsl:element>
        <xsl:element name="year">
          <xsl:if test="$program_current/*[local-name()='DATE'] != ''">
            <xsl:value-of select="string(floor(number(string($program_current/*[local-name()='DATE']))))"/>
          </xsl:if>
        </xsl:element>
        <xsl:if test="$program_current/*[local-name()='SERIES']/*[local-name()='GN_ID'] != ''">
          <xsl:element name="series">
            <xsl:value-of select="string($program_current/*[local-name()='SERIES']/*[local-name()='GN_ID'])"/>
          </xsl:element>
        </xsl:if>
        <xsl:if test="$program_current/*[local-name()='EPISODE_NUM'] != ''">
          <xsl:element name="syndicatedEpisodeNumber">
            <xsl:value-of select="string($program_current/*[local-name()='EPISODE_NUM'])"/>
          </xsl:element>
        </xsl:if>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template name="create_costumFields">
    <xsl:param name="program_current"/>
    <xsl:if test="$program_current/*[local-name()='URLGROUP']/*[local-name()='URL'] != ''">
      <xsl:element name="custom" xmlns="urn:TMSWebServices">
        <xsl:attribute name="program" >
          <xsl:value-of select="string($program_current/*[local-name()='GN_ID'])"/>
        </xsl:attribute>
        <xsl:element name="thumbnails">
          <xsl:element name="thumbnail">
            <xsl:value-of select="string($program_current/*[local-name()='URLGROUP']/*[local-name()='URL'])"/>
          </xsl:element>
        </xsl:element>
      </xsl:element>
    </xsl:if>
  </xsl:template>

</xsl:stylesheet>

